using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine.UIElements;

public class AddSpritesToAnimation : EditorWindow
{
    private string path = "Assets/png";
    private float frameRate = 60f; // 设定帧率为60fps，可以根据需要调整
    private AnimatorController animatorController;
    private List<Sprite> sprites = new();

    [MenuItem("Tools/Add Sprites to Animation")]
    static void Init()
    {
        EditorWindow.GetWindow(typeof(AddSpritesToAnimation)).Show();
    }

    void OnGUI()
    {
        animatorController = EditorGUILayout.ObjectField("Animator Controller", animatorController, typeof(AnimatorController), false) as AnimatorController;
        path = EditorGUILayout.TextField("Path", path);
        frameRate = EditorGUILayout.FloatField("FPS", frameRate);
        if (GUILayout.Button("Load Sprites"))
        {
            LoadSpritesFromFolder(path);
        }
        if (GUILayout.Button("Add To Animation") && animatorController != null)
        {
            AddSpritesToAnimator();
        }
    }

    private void LoadSpritesFromFolder(string path)
    {
        sprites.Clear();
        string[] fileEntries = Directory.GetFiles(path, "*.png", SearchOption.AllDirectories);
        foreach (string fileName in fileEntries)
        {
            Sprite sprite = AssetDatabase.LoadAssetAtPath<Sprite>(fileName);
            if (sprite != null)
            {
                sprites.Add(sprite);
            }
        }

        if (sprites.Count == 0)
        {
            Debug.Log("No sprites found in the folder!");
        }
        else
        {
            Debug.Log(sprites.Count + " sprites loaded from folder!");
        }
    }


    private void AddSpritesToAnimator()
    {
        var animationClips = animatorController.animationClips;
        if (animationClips.Length == 0)
        {
            Debug.Log("No animation clip found in the animator!");
            return;
        }

        var clip = animationClips[0];
        var editorCurveBinding = EditorCurveBinding.PPtrCurve("", typeof(SpriteRenderer), "m_Sprite");

        ObjectReferenceKeyframe[] keyframes = new ObjectReferenceKeyframe[sprites.Count];
        float frameTime = 0f;

        for (int i = 0; i < sprites.Count; i++)
        {
            keyframes[i] = new ObjectReferenceKeyframe
            {
                time = frameTime,
                value = sprites[i]
            };
            frameTime += 1 / frameRate;
        }

        AnimationUtility.SetObjectReferenceCurve(clip, editorCurveBinding, keyframes);
    }

}
