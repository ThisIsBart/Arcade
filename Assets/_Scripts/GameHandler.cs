using System.Collections.Generic;
using TMPro;
using UnityEngine;
using System.IO;
using System.Diagnostics;
using System.Text.RegularExpressions;

public class GameHandler : MonoBehaviour
{
    //The fields where the names of the game and team as well as the description of the game are displayed
    public TMP_Text nameTextFieldOrange;
    public TMP_Text nameTextFieldWhite;
    public TMP_Text teamTextFieldOrange;
    public TMP_Text teamTextFieldWhite;
    public TMP_Text descriptionTextField;
    //The images were the pictures go. Thumbnails are prefabs, 1 per game.
    public UnityEngine.UI.Image backgroundImage;
    public UnityEngine.UI.Image thumbnailImage;
    public Texture2D defaultThumbnail;
    //This is where the thumbnails go
    public GameObject thumbnailArray;
    //The height and width the images should be
    private int BACKGROUND_IM_HEIGHT = 1920;
    private int BACKGROUND_IM_WIDTH = 893;
    private int THUMBNAIL_IM_SIZE_UNSELECTED = 150;
    private int THUMBNAIL_IM_SIZE_SELECTED = 242;
    private Color SINT_LUCAS_ORANGE = new Color(255/255f, 102/255f, 0/255f, 1f);
    //The index of the currently selected game
    private int cycle = 1;

    //Keep all gamefolders, game names, team names, game descriptions, background images, thumbnails and (paths to) the
    //executables in memory. The idea is that all matching elements are at the same index in their respective array,
    //this way you can just call element 'x' of every array.
    //No, I did not make it object oriented
    DirectoryInfo[] folders;
    string[] gameNames;
    string[] teamNames;
    string[] descriptions;
    Sprite[] images;
    UnityEngine.UI.Image[] thumbnails;
    string[] executables;

    // Read all the folders, then write all the games' names, team names, background images, thumbnails, descriptions and
    // paths to executables to their respective arrays. The idea is that these, more expensive operations, will be done once,
    // when the manager (this program) starts
    void Awake()
    {
        //Fill folders array and create the other arrays to have the same size
        folders = new DirectoryInfo("./_Games").GetDirectories();
        gameNames = new string[folders.Length];
        teamNames = new string[folders.Length];
        descriptions = new string[folders.Length];
        images = new Sprite[folders.Length];
        thumbnails = new UnityEngine.UI.Image[folders.Length];
        executables = new string[folders.Length];

        //Displaying the game and team name is split into an orange text field and a white text field
        nameTextFieldOrange.color = SINT_LUCAS_ORANGE;
        nameTextFieldWhite.color = Color.white;
        teamTextFieldOrange.color = SINT_LUCAS_ORANGE;
        teamTextFieldWhite.color = Color.white;


        //For each folder(gamefolder as delivered by the students) in folders, put their information in their respective place in
        //every array
        for (int i = 0; i < folders.Length; i++)
        {
            //Open Directory 'i' and get all get all files in 'i'
            DirectoryInfo currentDir = new DirectoryInfo("./_Games/" + folders[i].Name);
            FileInfo[] files = currentDir.GetFiles();

            //For directory 'i' write the game name, team name, description, background image, thumbnail and executable to the array
            gameNames[i] = currentDir.Name;
            for (int j = 0; j < files.Length; j++)
            {
                //Put the contents of the developer.txt file at place 'i' in teamNames (removing end lines)
                if (files[j].Name.ToLower().Equals("developer.txt"))
                {
                    StreamReader str = new StreamReader("./_Games/" + folders[i].Name + "/" + files[j].Name);
                    teamNames[i] = str.ReadToEnd();
                    str.Close();
                    teamNames[i] = Regex.Replace(teamNames[i], @"\t|\n|\r", " ");
                }
                //Put the contents of the descriptions.txt file at place 'i' in descriptions. Give the students a character limit
                //or this runs underneath the thumbnails.
                if (files[j].Name.ToLower().Equals("description.txt"))
                {
                    StreamReader str = new StreamReader("./_Games/" + folders[i].Name + "/" + files[j].Name);
                    descriptions[i] = str.ReadToEnd();
                    str.Close();
                }
                //Convert the background image to a sprite (via Texture2D) at place 'i' in images
                if (files[j].Name.ToLower().Equals("background.jpg"))
                {
                    Texture2D asTexture = new Texture2D(BACKGROUND_IM_HEIGHT, BACKGROUND_IM_WIDTH);
                    asTexture.LoadImage(File.ReadAllBytes("./_Games/" + gameNames[i] + "/" + files[j].Name));
                    images[i] = Sprite.Create(asTexture, new Rect(0, 0, asTexture.width, asTexture.height), new Vector2(0, 0), 100, 0, SpriteMeshType.Tight);
                }
                //Convert the thumbnail image to a sprite (via Texture2D) at place 'i' in images
                if (File.Exists("./_Games/" + gameNames[i] + "/" + "thumbnail.jpg"))
                {
                    if (files[j].Name.ToLower().Equals("thumbnail.jpg"))
                    {
                        Texture2D asTexture = new Texture2D(THUMBNAIL_IM_SIZE_UNSELECTED, THUMBNAIL_IM_SIZE_UNSELECTED);
                        asTexture.LoadImage(File.ReadAllBytes("./_Games/" + gameNames[i] + "/" + "thumbnail.jpg"));
                        thumbnails[i] = Instantiate(thumbnailImage, thumbnailArray.transform.position, Quaternion.identity);
                        thumbnails[i].sprite = Sprite.Create(asTexture, new Rect(0, 0, asTexture.width, asTexture.height), new Vector2(0, 0), 100, 0, SpriteMeshType.Tight);
                        thumbnails[i].transform.SetParent(thumbnailArray.transform);
                        thumbnails[i].transform.localScale = new Vector2(THUMBNAIL_IM_SIZE_UNSELECTED, THUMBNAIL_IM_SIZE_UNSELECTED);
                        thumbnails[i].canvasRenderer.SetAlpha(0.5f);
                    }
                }
            }
            //For every file in the 'Builds' (or 'build' as people keep renaming the damn thing) map check if it's the executable,
            //if so add it to executables at place 'i'
            string pathEnd = "Builds";
            if (!Directory.Exists("./_Games/" + folders[i].Name + "/" + "Builds"))
                pathEnd = "build";
            DirectoryInfo build = new DirectoryInfo("./_Games/" + folders[i].Name + "/" + "Builds");
            foreach (FileInfo thing in build.GetFiles())
            {   //get the .exe that is not the crashHandler
                if (thing.Name.EndsWith(".exe") && !(thing.Name.Equals("UnityCrashHandler64.exe") || thing.Name.Equals("UnityCrashHandler32.exe")))
                {
                    executables[i] = folders[i] + "/" + pathEnd + "/" + thing.Name;
                }
            }
            //If there isn't a thumbnail, use the default one
            if (!File.Exists("./_Games/" + gameNames[i] + "/" + "thumbnail.jpg"))
            {
                Texture2D asTexture = new Texture2D(THUMBNAIL_IM_SIZE_UNSELECTED, THUMBNAIL_IM_SIZE_UNSELECTED);
                asTexture.LoadImage(File.ReadAllBytes("./Assets/Materials/defaultThumbnail.jpg"));
                thumbnails[i] = Instantiate(thumbnailImage, thumbnailArray.transform.position, Quaternion.identity);
                thumbnails[i].sprite = Sprite.Create(asTexture, new Rect(0, 0, asTexture.width, asTexture.height), new Vector2(0, 0), 100, 0, SpriteMeshType.Tight);
                thumbnails[i].transform.SetParent(thumbnailArray.transform);
                thumbnails[i].transform.localScale = new Vector2(THUMBNAIL_IM_SIZE_UNSELECTED, THUMBNAIL_IM_SIZE_UNSELECTED);
                thumbnails[i].canvasRenderer.SetAlpha(0.5f);
            }
        }
        //initialise everything to have the second game selected
        setName(gameNames[cycle], nameTextFieldOrange, nameTextFieldWhite);
        if (teamNames[cycle] != null)
        {
            setName(teamNames[cycle], teamTextFieldOrange, teamTextFieldWhite);
        }
        else
        {
            setName("  ", teamTextFieldOrange, teamTextFieldWhite);
        }
        descriptionTextField.text = descriptions[cycle];
        backgroundImage.sprite = images[cycle];
        thumbnails[cycle].transform.localScale = new Vector2(THUMBNAIL_IM_SIZE_SELECTED, THUMBNAIL_IM_SIZE_SELECTED);
        thumbnails[cycle].canvasRenderer.SetAlpha(1f);

    }


    // Update is called once per frame
    void Update()
    {
        //When right is pressed set all relevant fields to the next game and cycle all the thumbnails one place to the left. 
        if (Input.GetKeyDown(KeyCode.D))
        {
            cycle++;
            cycle %= gameNames.Length;
            //turn the first half of the game and team name orange, the rest white
            setName(gameNames[cycle], nameTextFieldOrange, nameTextFieldWhite);
            //Some games don't have a team name so we'll check for that
            if (teamNames[cycle] != null)
            {
                setName(teamNames[cycle], teamTextFieldOrange, teamTextFieldWhite);
            }
            else
            {
                setName("  ", teamTextFieldOrange, teamTextFieldWhite);
            }
            descriptionTextField.text = descriptions[cycle];
            backgroundImage.sprite = images[cycle];
            

            //shrink thumbnail in second space
            thumbnails[1].transform.localScale = new Vector2(THUMBNAIL_IM_SIZE_UNSELECTED, THUMBNAIL_IM_SIZE_UNSELECTED);
            thumbnails[1].canvasRenderer.SetAlpha(0.5f);
            //copy first element to separate space
            UnityEngine.UI.Image tempImage = thumbnails[0];
            //copy every element to their position -1
            for(int i = 1; i < thumbnails.Length; i++)
            {
                thumbnails[i-1] = thumbnails[i];
            }
            //set last element to copied element
            thumbnails[thumbnails.Length-1] = tempImage;
            //grow sprite in second space
            thumbnails[1].transform.localScale = new Vector2(THUMBNAIL_IM_SIZE_SELECTED, THUMBNAIL_IM_SIZE_SELECTED);
            thumbnails[1].canvasRenderer.SetAlpha(1f);
            //move the first thumbnail to the back
            thumbnailArray.transform.GetChild(0).SetSiblingIndex(thumbnailArray.transform.childCount - 1);

        }
        //When left is pressed set all relevant fields to the previous game and cycle all the thumbnails one place to the right. 
        if (Input.GetKeyDown(KeyCode.A))
        {
            cycle--;
            if (cycle < 0) 
            { 
                cycle = gameNames.Length-1; 
            }
            setName(gameNames[cycle], nameTextFieldOrange, nameTextFieldWhite);
            if (teamNames[cycle] != null)
            {
                setName(teamNames[cycle], teamTextFieldOrange, teamTextFieldWhite);
            }
            else
            {
                setName("  ", teamTextFieldOrange, teamTextFieldWhite);
            }
            descriptionTextField.text = descriptions[cycle];
            backgroundImage.sprite = images[cycle];

            //shrink thumbnail in second space
            thumbnails[1].transform.localScale = new Vector2(THUMBNAIL_IM_SIZE_UNSELECTED, THUMBNAIL_IM_SIZE_UNSELECTED);
            thumbnails[1].canvasRenderer.SetAlpha(0.5f);
            //copy last element to separate space
            UnityEngine.UI.Image tempImage = thumbnails[thumbnails.Length-1];
            //copy every element to their position +1
            for (int i = thumbnails.Length-1; i > 0; i--)
            {
                thumbnails[i] = thumbnails[i-1];
            }
            //set last element to copied element
            thumbnails[0] = tempImage;
            //grow sprite in second space
            thumbnails[1].transform.localScale = new Vector2(THUMBNAIL_IM_SIZE_SELECTED, THUMBNAIL_IM_SIZE_SELECTED);
            thumbnails[1].canvasRenderer.SetAlpha(1f);
            //move the last thumbnail to the front
            thumbnailArray.transform.GetChild(thumbnailArray.transform.childCount-1).SetSiblingIndex(0);

        }

        //When the red button is pressed: run the executable
        if (Input.GetKeyDown(KeyCode.R))
        {
            Process.Start(executables[cycle]);
        }

        //When you press up bring up the menu
        //start of program: for each year folder, do what we do now
        //unless it's done reeeeeeaaaaally ugly requires restructuring of all the files array(years) of arrays(info)?
                                                                           //alternatively, array(years) of objects(rewritten games)
        //select the latest year as default

    }

    //split name off into 2. If possible split on spaces otherwise on half of the characters
    private void setName(string name, TMP_Text orange, TMP_Text white)
    {
        //put index of all whitespaces in 'spaces'
        List<int> spaces = new List<int>();
        for (int i = 0; i < name.Length; i++)
        {
            if (name[i] == ' ')
            {
                spaces.Add(i);
            }
        }
        //if there are spaces in the name (i.e. the name consists of multiple words) change color to white after
        //half the words (rounded up)
        if (spaces.Count > 0)
        {
            orange.text = name.Substring(0, spaces[spaces.Count / 2]);
            white.text = name.Substring(spaces[spaces.Count / 2]);
        }
        //if there are no spaces change color after half the name's length
        else
        {
            orange.text = name.Substring(0, name.Length / 2);
            white.text = name.Substring(name.Length / 2);
        }
        //set the location of the white text to the location of the orange text + it's preferred width (determined by the width of it's current text)
        white.transform.SetLocalPositionAndRotation(orange.transform.localPosition + new Vector3(orange.preferredWidth, 0, 0), white.transform.rotation);
    }
}
