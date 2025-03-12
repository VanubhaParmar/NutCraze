using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class MainScene : MonoBehaviour
{
    private readonly Stack<Texture2D> _textures = new Stack<Texture2D>();
    private readonly Stack<byte[]> _data = new Stack<byte[]>();

    //public Canvas canvas;
    public Button arrayButton;
    public Button textureButton;

    private const string STATUS_MESSAGE = "Memory State: ";
    private const string OTHER_MEMORY_MESSAGE = "Others Memory: ";
    private const string NATIVE_MEMORY_MESSAGE = "Native Memory: ";
    private const string GRAPHIC_MEMORY_MESSAGE = "Graphics Memory: ";
    private const string TOTAL_MEMORY_MESSAGE = "Total Memory: ";
    private const string LAST_ACTION_MESSAGE = "Last action: ";
    private readonly string ADD_ARRAY_MESSAGE = "Allocating array " + ARRAY_SIZE + "MB to Others Memory";
    private readonly string REMOVE_ARRAY_MESSAGE = "Removing array " + ARRAY_SIZE + "MB from Others Memory";
    private readonly string ADD_TEXTURE_MESSAGE = "Allocating " + NUM_TEXTURE + " Texture2D, "
        + NUM_TEXTURE * TEXTURE_SIZE_MB + "MB to Graphics";
    private readonly string REMOVE_TEXTURE_MESSAGE = "Removing " + NUM_TEXTURE + " Texture2D, "
        + NUM_TEXTURE * TEXTURE_SIZE_MB + "MB from Graphics";
    private const string MB_MESSAGE = "MB";
    private const string RUNNING_MESSAGE = "RUNNING, PLEASE WAIT...";

    private const int MB_SIZE = 1024 * 1024;
    private const int ARRAY_SIZE = 200;
    private const int TEXTURE_WIDTH = 4096;
    private const int TEXTURE_HEIGHT = 4096;
    private const int TEXTURE_BPP = 4;
    private const int TEXTURE_SIZE_MB = ((TEXTURE_WIDTH * TEXTURE_HEIGHT * TEXTURE_BPP) / MB_SIZE);
    private const int NUM_TEXTURE = 4;

    private long otherMem = 0;
    private long nativeMem = 0;
    private long graphicsMem = 100;
    private Action lastAction = Action.Init;

    private enum Action
    {
        Init, AddArray, RemoveArray, AddTexture, RemoveTexture
    }

    public Text statusText;
    public Text otherMemoryText;
    public Text graphicsMemoryText;
    public Text nativeMemoryText;
    public Text totalMemoryText;
    public Text actionText;

    // Start is called before the first frame update
    void Start()
    {
//#if UNITY_ANDROID && !UNITY_EDITOR
        //MemoryAdviceErrorCode errorCode = MemoryAdvice.Init();
        //if(errorCode == MemoryAdviceErrorCode.Ok)
        //{
        //    errorCode = MemoryAdvice.RegisterWatcher(5000, new MemoryWatcherDelegateListener((MemoryState state) =>
        //    {
        //        Debug.LogError("Watcher update Memory State: " + state.ToString());
        //        UpdateTextStatus();
        //    }));

        //    if(errorCode == MemoryAdviceErrorCode.Ok)
        //    {
        //        Debug.LogError("Memory advice watcher registered successfully");
        //    }
        //}
//#endif
        UpdateText();
    }

    // Update is called once per frame
    public void OnClickAllocateCharacterButton()
    {
        Debug.LogError("Start allocating Character Array");
        actionText.text = RUNNING_MESSAGE;
        AllocateCharacterArray();
        UpdateText();
    }

    public void OnClickRemoveCharacterButton()
    {
        if(_data.Count > 0)
        {
            Debug.LogError("Remove allocated array");
            actionText.text = RUNNING_MESSAGE;
            _data.Pop();            
            otherMem -= ARRAY_SIZE;
            lastAction = Action.RemoveArray;
            System.GC.Collect();
            UpdateText();
        }
    }

    public void OnClickAllocateTextureButton()
    {
        Debug.LogError("Start allocating Texture2D");
        actionText.text = RUNNING_MESSAGE;
        for (int i = 0; i < NUM_TEXTURE; i++){
            CreateTexture2D();
        }
        UpdateText();
    }

    public void OnClickRemoveTextureButton()
    {
        if(_textures.Count >= NUM_TEXTURE)
        {
            actionText.text = RUNNING_MESSAGE;
            for (int i = 1; i <= NUM_TEXTURE; i++)
            {
                Texture2D temp = _textures.Pop();
                Texture2D.Destroy(temp);
                graphicsMem -= TEXTURE_SIZE_MB;
            }
            lastAction = Action.RemoveTexture;
            System.GC.Collect();
            UpdateText();
        }
    }

    public void UpdateText()
    {
        otherMemoryText.text = OTHER_MEMORY_MESSAGE + otherMem + MB_MESSAGE;
        nativeMemoryText.text = NATIVE_MEMORY_MESSAGE + nativeMem + MB_MESSAGE;
        graphicsMemoryText.text = GRAPHIC_MEMORY_MESSAGE + graphicsMem + MB_MESSAGE;
        totalMemoryText.text = TOTAL_MEMORY_MESSAGE + (otherMem + nativeMem + graphicsMem).ToString() + MB_MESSAGE;
        actionText.text = LAST_ACTION_MESSAGE;
        switch (lastAction)
        {
            case Action.AddArray:
                actionText.text += ADD_ARRAY_MESSAGE;
                break;
            case Action.AddTexture:
                actionText.text += ADD_TEXTURE_MESSAGE;
                break;
            case Action.RemoveArray:
                actionText.text += REMOVE_ARRAY_MESSAGE;
                break;
            case Action.RemoveTexture:
                actionText.text += REMOVE_TEXTURE_MESSAGE;
                break;
            case Action.Init:
                actionText.text += "INIT APPLICATION";
                break;
        }
        UpdateTextStatus();
    }

    private void UpdateTextStatus()
    {        
        Color color = Color.white;
//#if UNITY_ANDROID && !UNITY_EDITOR
        //MemoryState memoryState = MemoryAdvice.GetMemoryState();
        //switch (memoryState)
        //{
        //    case MemoryState.ApproachingLimit:
        //        statusText.text = STATUS_MESSAGE + "APPROACHING_LIMIT";
        //        color = Color.yellow;
        //        break;
        //    case  MemoryState.Critical:
        //        statusText.text = STATUS_MESSAGE + "MEMORY_CRITICAL";
        //        color = Color.red;
        //        break;
        //    default:
        //        statusText.text = STATUS_MESSAGE + "OK";
        //        break;
        //}
//#endif
        statusText.color = color;
        otherMemoryText.color = color;
        nativeMemoryText.color = color;
        graphicsMemoryText.color = color;
        totalMemoryText.color = color;
        actionText.color = color;
    }

    private void CreateTexture2D()
    {
        Debug.LogError("Create a new Texture2D, current count = " + _textures.Count);
        Texture2D texture = new Texture2D(TEXTURE_WIDTH, TEXTURE_HEIGHT, TextureFormat.RGBA32, false);        
        texture.Apply(false, true);

        _textures.Push(texture);
        graphicsMem += TEXTURE_SIZE_MB;
        lastAction = Action.AddTexture;
    }

    public void AllocateCharacterArray()
    {
        Debug.LogError("Allocate :" + ARRAY_SIZE + "MB");   
        byte[] temp = new byte[MB_SIZE*ARRAY_SIZE];
        _data.Push(temp);
        otherMem += ARRAY_SIZE;
        lastAction = Action.AddArray;
    }
}