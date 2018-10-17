/***
 * Very basic class for a Player Manager  
 * */
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour {

    //playing locked animation
    bool locked = false;
    //playing animation
    bool in_animation = false;

    //Buffering some components
    InputsInfo info;
    InputManager inputManager;
    
    //frame rate
    public float period = 1f / 60f;
    private float nextActionTime = 0.0f;

    //For command sequences
    public int framesToConnectSequence = 16;
    int framesSinceMotionEnded = 0;
    string currentSequence;

    //One command can be buffered during animation
    InputEntryInfo bufferedCommand = null;
    int framesSinceLastBufferUpdate = 0;

    // Use this for initialization
    void Start () {
        info = gameObject.GetComponent<InputsInfo>();
        inputManager = gameObject.GetComponent<InputManager>();
        nextActionTime = Time.time + period;
    }
   
    //Called when animation finish
    public void MotionEnds()
    {
        locked = false;
        in_animation = false;

        framesSinceMotionEnded = framesToConnectSequence;
    }

    //Detects if we are in a sequence and plays the adequate motion
    public bool CheckSequence(string next)
    {
        currentSequence += next + " ";
        if (framesSinceMotionEnded <= 0 || next == "") {
            return false;
        }
      
        bool inSequence = false;
        var trimSequence = currentSequence.TrimEnd(' ');
      
        if (trimSequence.Length == 1) return false;
        foreach (var seq in info.Sequences)
        {
            if (seq.input.StartsWith(trimSequence))
            {
                inSequence = true;
                if (trimSequence == seq.input)
                {                   
                    locked = !seq.isCancelable;
                    in_animation = true;
                    gameObject.GetComponent<Animator>().SetTrigger(seq.animationTrigger);                    
                    return true;
                }
            }
        }

        //Reset sequence string
        if (!inSequence)
        {           
            currentSequence = "";
        }
        return false;
    }
    
    //We can save one basic command during animation for playing at the end
    void UpdateInAnimationBuffer()
    {
        if (in_animation)
        {
            framesSinceLastBufferUpdate++;
            foreach (var cmd in info.BasicCommands)
            {
                if (inputManager.IsPressedInFrameWindow(cmd.input))
                {
                    Debug.Log("Buffered " + cmd.input);
                    bufferedCommand = cmd;
                    framesSinceLastBufferUpdate = 0;
                }
            }
        }
        else
        {
            bufferedCommand = null;
        }
    }

    private void Update()
    {
        //Update inputs
        inputManager.UpdateInputBuffer();
    }

    void FixedUpdate()
    {
       
        int i = 0;
        //Ensuring one update per frame, if slower we will update as many frames as needed
        // (but probably input will be a mess)
        while (Time.time > nextActionTime)
        {
            i++;

            //Input frame update
            inputManager.UpdateInputs();
            //Check if user inputs for a command
            var result = inputManager.FindCommandInBuffer();
            
            DoFrame(result);

            UpdateInAnimationBuffer();

            //Next update time
            nextActionTime += period;

            //Clear buffer
            inputManager.ClearKeys();
        }
        //if (i>1)
         //Debug.Log("Executed " + i + " times");
    }


    // Character frame update (Dummy, probably you want do this with a state machine!)
    public void DoFrame(InputEntryInfo command)
    {
        //If already in a locked animation, do nothing
        if (!locked)
        {
            //Special skill
            if (command != null && (!in_animation || command.isSpecialSkill ))
            {
                locked = !command.isCancelable;
                in_animation = true;
                gameObject.GetComponent<Animator>().SetTrigger(command.animationTrigger);
                inputManager.ClearKeys();
            }
            //Basic command or sequence
            else
            {
                //Basic commands can't start in the middle of an animation
                if (!in_animation)
                {
                    var found = false;
                    //Check basic commmands
                    foreach(var cmd in info.BasicCommands)
                    {
                        if (inputManager.IsPressedInFrameWindow(cmd.input) || cmd == bufferedCommand)
                        {
                            // If not in sequence, do standard things
                            if (!CheckSequence(cmd.code))
                            {
                                Debug.Log("Play basic " + cmd.input);
                                locked = !cmd.isCancelable; in_animation = true;
                                gameObject.GetComponent<Animator>().SetTrigger(cmd.animationTrigger);
                               
                            }
                            bufferedCommand = null;
                            found = true;
                            inputManager.ClearKeys();
                        }
                    }

                    //Sequence reset after some frames
                    if (!found && framesSinceMotionEnded > 0)
                    {
                        framesSinceMotionEnded--;
                        if (framesSinceMotionEnded == 0)
                        {                           
                            currentSequence = "";
                        }
                    }

                    
                }
            }
          
           
        }
    }
	
}
