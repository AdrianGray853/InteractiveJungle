using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Interactive.PuzzelShape
{
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

    //Copy and paste atlas settings to another atlas editor
    public class SpriteCopy : EditorWindow
    {

    	public Texture2D copyFrom;           //Sprite Atlas to copy from settings
    	public Texture2D copyTo;           //Sprite atlas where to paste settings

    	private Sprite[] _sprites;           //Collection of sprites from source texture for faster referencing

    	[MenuItem("Window/Sprite Copy Data")]
    	static void Init()
    	{
    		// Window Set-Up
    		SpriteCopy window = EditorWindow.GetWindow(typeof(SpriteCopy), false, "Atlas Editor", true) as SpriteCopy;
    		window.minSize = new Vector2(260, 170); window.maxSize = new Vector2(260, 170);
    		window.Show();
    	}

    	//Show UI
    	void OnGUI()
    	{

    		copyFrom = (Texture2D)EditorGUILayout.ObjectField("Copy From", copyFrom, typeof(Texture2D), true);
    		copyTo = (Texture2D)EditorGUILayout.ObjectField("Paste To", copyTo, typeof(Texture2D), true);

    		EditorGUILayout.Space();

    		if (GUILayout.Button("Copy Paste"))
    		{
    			if (copyFrom != null && copyTo != null)
    				CopyPaste();
    			else
    				Debug.LogWarning("Forgot to set the textures?");
    		}

    		//Repaint();
    	}

    	//Do the copy paste
    	private void CopyPaste()
    	{
    		if (!copyFrom || !copyTo)
    		{
    			Debug.Log("Please assign some sprites!");
    			return;
    		}

    		if (copyFrom.GetType() != typeof(Texture2D) || copyTo.GetType() != typeof(Texture2D))
    		{
    			Debug.Log("Cant convert from: " + copyFrom.GetType() + "to: " + copyTo.GetType() + ". Needs two Texture2D objects!");
    			return;
    		}

    		if (copyFrom.width != copyTo.width || copyFrom.height != copyTo.height)
    		{
    			//Better a warning if textures doesn't match than a crash or error
    			Debug.LogWarning("Unable to proceed, textures size doesn't match.");
    			return;
    		}

    		if (!IsAtlas(copyFrom))
    		{
    			Debug.LogWarning("Unable to proceed, the source texture is not a sprite atlas.");
    			return;
    		}

    		string copyFromPath = AssetDatabase.GetAssetPath(copyFrom);
    		TextureImporter ti1 = AssetImporter.GetAtPath(copyFromPath) as TextureImporter;
    		ti1.isReadable = true;

    		string copyToPath = AssetDatabase.GetAssetPath(copyTo);
    		TextureImporter ti2 = AssetImporter.GetAtPath(copyToPath) as TextureImporter;
    		ti2.isReadable = true;

    		ti2.textureType = TextureImporterType.Sprite;
    		ti2.spriteImportMode = SpriteImportMode.Multiple;

    		List<SpriteMetaData> newData = new List<SpriteMetaData>();

    		Debug.Log("Amount of slices found: " + ti1.spritesheet.Length);

    		for (int i = 0; i < ti1.spritesheet.Length; i++)
    		{
    			SpriteMetaData d = ti1.spritesheet[i];
    			newData.Add(d);
    		}
    		ti2.spritesheet = newData.ToArray();
    		//ti2.SaveAndReimport();

    		//ti2.isReadable = false;
    		AssetDatabase.ImportAsset(copyToPath, ImportAssetOptions.ForceUpdate);
    		//ti2.isReadable = true;
    		AssetDatabase.Refresh();
    	}

    	//Check that the texture is an actual atlas and not a normal texture
    	private bool IsAtlas(Texture2D tex)
    	{
    		string _path = AssetDatabase.GetAssetPath(tex);
    		TextureImporter _importer = AssetImporter.GetAtPath(_path) as TextureImporter;

    		return _importer.textureType == TextureImporterType.Sprite && _importer.spriteImportMode == SpriteImportMode.Multiple;
    	}
    }

}