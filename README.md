<h1 align="center">Pinned Assets</h1>

<h4 align="center">Unity Extension allowing for convenient pinned lists of assets, scene gameobjects and components.<br><br>
 Select anything instantly.</h4>

<p align="center">
  <a href="#about">About</a> •
  <a href="#installation">Installation</a> •
  <a href="#support">Support</a> •
  <a href="#donate">Donate</a>
</p>

<p align="center">
 <img width="641" height="267" alt="image" src="https://github.com/user-attachments/assets/821fa19b-6cb7-44aa-8414-c81494d5bd47" />
</p>

## About

Pinned Assets was made as Unity does not have an easy way to pin frequently accessed assets.
This allows you to not only pin assets, but also make multiple profiles to easily group what you need together.

### API 

Custom GUI drawers can be created for asset types, to extend pinned functionality:

<p align="center">
 <img width="620" height="76" alt="image" src="https://github.com/user-attachments/assets/73caed9d-62c8-4314-a9ab-4dd354fd4000" />
</p>

```C#
public class TextAssetDrawer : PinnedAssetDrawer<TextAsset>
{
    protected override void OnAssetGUI(Rect rect, TextAsset asset, PinnedAssetListData list, SerializedObject serializedObject)
    {
        DrawDefaultGUI(rect, asset, list, serializedObject);

        if (Application.isPlaying)
        {
            return;
        }

        if (Button(rect, Icons.Edit, Styles.ToolbarButton, 64f))
        {
            OpenScript(asset);
        }
    }

    private void OpenScript(TextAsset asset)
    {
        AssetDatabase.OpenAsset(asset.GetInstanceID());
    }

    public override bool IsValid(TextAsset instance)
    {
        return base.IsValid(instance);
    }
}
 ```

## Installation
<p align="center">
  <a href="https://github.com/WooshiiDev/PinnedAssets/releases">Releases</a> •
  <a href="https://github.com/WooshiiDev/PinnedAssets/archive/master.zip">Zip</a> 
</p>

This can be installed directly through the git url
```
https://github.com/WooshiiDev/PinnedAssets.git
```

You can also install this via git by adding the following to your **manifest.json**
```
"com.wooshii.pinnedassets" : "https://github.com/WooshiiDev/PinnedAssets.git"
```


## Support
Please submit any queries, bugs or issues, to the [Issues](https://github.com/WooshiiDev/PinnedAssets/issues) page on this repository. All feedback is appreciated as it helps improves Pinned Assets.

Reach out to me or see my other work through:

 - Website: https://wooshii.dev/
 - Email: wooshiidev@gmail.com;

## Donate
Pinned Assets will be and always has been developed in my free time. If you would to support me, you can do so below:

[![PayPal](https://www.paypalobjects.com/en_US/i/btn/btn_donateCC_LG.gif)](https://paypal.me/Wooshii?locale.x=en_GB)
