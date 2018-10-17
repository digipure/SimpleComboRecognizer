
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class InputManager : MonoBehaviour
{
    // How many frames for recognizing two inputs are made at the same time
    public int frameWindow = 3;
    //Min/max distance between inputs from recognizing special skills
    public int min_distance = 0;
    public int max_distance = 2;

    //Current input result
    string result = "";    
    //For outputing the result
    public Text t;

    //Copy & paste a full buffer here for replay (probably buggy)
    public bool playbackMode = false;
    public bool recordMode = false;
    public string play_buffer = "";
    //All the inputs are saved here
    public string record_buffer = "";
    //index for playing play buffer
    int r_index = 0;

    //Counter for the frame window
    int blank_counter = 0;
    //Currently pressed keys as a string
    string frameWindowWord = "";
    //Currently pressed keys
    Dictionary<string, bool> keysPressed = new Dictionary<string, bool>();
    //Check if a key is pressed currently
    public bool IsPressed(string cmd) { return (keysPressed.ContainsKey(cmd) && keysPressed[cmd]); }
    //Check if a key is pressed in current word (frame window)
    public bool IsPressedInFrameWindow(string s) { return frameWindowWord.Contains(s); }
    //Save the character commands info
    InputsInfo info;

    // Available keys
    Dictionary<string, KeyCode> keys_to_check = new Dictionary<string, KeyCode>()
    {
        {"P", KeyCode.Q},
        {"K", KeyCode.W},

        {"U", KeyCode.UpArrow},
        {"D", KeyCode.DownArrow},
        {"L", KeyCode.LeftArrow},
        {"R", KeyCode.RightArrow},
    };

    void Start()
    {
        info = gameObject.GetComponent<InputsInfo>();
    }


    //Clear the currently pressed keys buffer
    public void ClearKeys()
    {
        frameWindowWord = "";

        //No keys or playback mode
        if (keysPressed.Count == 0)
            return;
        
        List<string> keys = new List<string>(keysPressed.Keys);
        foreach (var key in keys)
        {
            keysPressed[key] = false;
        }       
    }

    bool SomethingPressed()
    {
        foreach (var item in keysPressed)
        {
            if (item.Value) return true;
        }
        return false;
    }

    public string GetCurrentFrameWord()
    {
        string word = "";
        foreach (var item in keysPressed)
        {
            if (item.Value) word += item.Key;
        }
        return word;
    }

   

    public void UpdateInputBuffer()
    {
        foreach (var item in keys_to_check)
        {
            if (Input.GetKeyDown(item.Value)) keysPressed[item.Key] = true;
        }

        if (Input.GetButtonDown("Fire1")) keysPressed["P"] = true;
        if (Input.GetButtonDown("Fire2")) keysPressed["K"] = true;
    }
    
    //Input frame update
    public void  UpdateInputs()
    {              
        if (play_buffer.Length == 0 || !playbackMode)
        {
            frameWindowWord = GetCurrentFrameWord();
        }
        else
        {
            frameWindowWord = "";
            while (r_index < play_buffer.Length && play_buffer[r_index] != '_')
            {
                frameWindowWord += play_buffer[r_index];
                r_index++;
            }
            r_index++;
            if (r_index >= play_buffer.Length) r_index = 0;
        }

        frameWindowWord = frameWindowWord.Replace("_", "");
    
        result += frameWindowWord;

        if (recordMode)
            record_buffer += frameWindowWord;
      
        if (blank_counter != 0 || !result.All(c => c == '*'))
            blank_counter++;
        if (blank_counter > frameWindow)
        {
            result += "*";
            blank_counter = 0;
        }

        if (recordMode)
            record_buffer += "_";
    }

    //Look for commands
    public InputEntryInfo FindCommandInBuffer()
    {
        InputEntryInfo pattern = null;
      
        pattern = Parse(result);

        result = pattern != null && pattern.isSpecialSkill ? pattern.code : result;

        if (result.Length > 15) result = result.Substring(1);
        t.text = result.Replace("_", " ");

        return pattern;
    }

    //Try to find a valid command in the input string buffer
    InputEntryInfo Parse(string t)
    {
        string comparet = t.TrimEnd('*');

        if (t.Length == 0)
            return null;

        foreach (var item in info.Commands)
        {
            var parts = item.input.Split('*');
            var last = parts.Last();
            var m = "";
            foreach (var c in parts)
            {
                m += "[" + c + @"]+" + (!c.Equals(last) ? @"\*{" + min_distance + "," + max_distance + "}" : "");
            }
           
            if (Regex.Match(comparet, m).Success)
            {
                return item;
            }
        }
        return null;
    }
}
