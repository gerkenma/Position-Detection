using System.Collections;
using System.Collections.Generic;
using System;
using System.Globalization;
using System.IO;
using UnityEngine;

public class SatelliteOrbit : MonoBehaviour {
    //This script governs the behavior of the camera in the Unity environment. It functions akin to a satellite orbiting the Earth in real life.

    //Width and height of the pictures to be taken
    public int resWidth = 256;
    public int resHeight = 256;

    //Latitude and longitude variables
    public float lon = 0;
    public float lat = 0;

    //String to display what major region (continent or ocean) the camera is above
    public string region = "";

    public bool land;

    public int counter;

    private ArrayList config = new ArrayList();
    private float spinspeed;
    private float orbitspeed;
    private bool takePictures;

    private bool takeHiResShot = false;

    public float x = 0;
    public float y = 0;
    public float z = 0;

    public DateTime date = new DateTime(2018, 1, 1);
   
    //Method to create names for taken screenshots and output them to the appropriate directories. Change the string.Format() method as needed to suit the needs of your machine
    public static string ScreenShotName(int width, int height, string region)
    {
       return string.Format("{0}/screenshots/{1}/screen_{2}x{3}_{4}.png",
                                 Application.dataPath, region,
                                 width, height,
                                 System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss-fff", CultureInfo.InvariantCulture));
    }

    // Use this for initialization
    void Start () {
        counter = 0;
        land = false;

        using (var reader = new StreamReader("SimConfig.txt"))
        {
            string line;
            string[] values;

            while (!reader.EndOfStream)
            {
                line = reader.ReadLine();
                values = line.Split(' ');
                config.Add(values[2]);
            }
        }

        spinspeed = Convert.ToSingle(config[0]);
        orbitspeed = Convert.ToSingle(config[1]);

        if((string) config[2] == "Yes")
        {
            takePictures = true;
        }

        else
        {
            takePictures = false;
        }
	}

    // Update is called once per frame
    void Update()
    {
        //Method for the camera to spin while it is rotating around the Earth
        transform.Rotate(0, spinspeed * Time.deltaTime, 0, Space.World);

        //Method for the camera to rotate around the Earth
        //To change the orbit angle, change the second parameter to the desired Vector3
        transform.RotateAround(Vector3.zero, new Vector3(0, 0.56f, 0.44f), orbitspeed * Time.deltaTime);


        land = false;

        //Methods to find the longitude and latitude of the satellite
        lon = Vector3.SignedAngle(transform.position, Vector3.right, Vector3.up);
        lat = 90f - Vector3.Angle(transform.position, Vector3.up);

        x = transform.position.x;
        y = transform.position.y;
        z = transform.position.z;


        //This entire block is dedicated to checking if the latitude and longitude of the camera falls into certain coordinate ranges
        //If the camera falls into a certain coordinate range, then depending on the range, that shows what major region the camera
        //is above
        //All coordinate ranges are detailed in the Latitude Longitude Approximates Excel file

        //North America
        if (((lon >= -170.0f && lon < -145.0f) && (lat > 55.0f && lat <= 70.0f)) //Alaska
            || ((lon >= -145.0f && lon < -120.0f) && (lat > 55.0f && lat <= 70.0f)) //Alaska/Canada
            || ((lon >= -120.0f && lon < -65.0f) && (lat > 60.0f && lat <= 85.0f)) //Northern Canada
            || ((lon >= -65.0f && lon < 20.0f) && (lat > 60.0f && lat <= 85.0f)) //Greenland
            || ((lon >= -130.0f && lon < -60.0f) && (lat > 45.0f && lat <= 60.0f)) //Canada
            || ((lon >= -125.0f && lon < -70.0f) && (lat > 30.0f && lat <= 50.0f)) //USA
            || ((lon >= -110.0f && lon < -100.0f) && (lat > 10.0f && lat <= 30.0f)) // Mexico
            || ((lon >= -100.0f && lon < -85.0f) && (lat > 10.0f && lat <= 20.0f)) //Central America
            || ((lon >= -85.0f && lon < -70.0f) && (lat > 15.0f && lat <= 25.0f))) //Caribbean
        {
            land = true;
            region = "North_America";
        }

        //South Ameria
        else if (((lon >= -80.0f && lon < -30.0f) && (lat > 0.0f && lat <= 10.0f)) //Upper South America
            || ((lon >= -80.0f && lon < -35.0f) && (lat > -20.0f && lat <= 0.0f)) //Brazil
            || ((lon >= -70.0f && lon < -50.0f) && (lat > -40.0f && lat <= -20.0f)) //Argentina and part of Chile
            || ((lon >= -70.0f && lon < -63.0f) && (lat > -55.0f && lat <= -40.0f))) //Southern tip
        {
            land = true;
            region = "South_America";
        }

        //Europe
        else if (((lon >= 5.0f && lon < 40.0f) && (lat > 60.0f && lat <= 70.0f)) //Scandinavia
            || ((lon >= -10.0f && lon < 17.0f) && (lat > 35.0f && lat <= 60.0f)) //Western Europe
            || ((lon >= 7.0f && lon < 40.0f) && (lat > 35.0f && lat <= 60.0f)) //Eastern Europe
            || ((lon >= 40.0f && lon < 60.0f) && (lat > 40.0f && lat <= 70.0f))) //European part of Russia
        {
            land = true;
            region = "Europe";
        }

        //Asia
        else if (((lon >= 30.0f && lon < 75.0f) && (lat > 30.0f && lat <= 37.0f)) //Middle East
            || ((lon >= 35.0f && lon < 60.0f) && (lat > 10.0f && lat <= 30.0f)) //Arabian Peninsula
            || ((lon >= 60.0f && lon < 140.0f) && (lat > 40.0f && lat <= 75.0f)) //Siberia
            || ((lon >= 70.0f && lon < 120.0f) && (lat > 10.0f && lat <= 40.0f)) //Central/South/East Asia
            || ((lon >= 95.0f && lon < 155.0f) && (lat > -10.0f && lat <= 5.0f)) //Indonesia
            || ((lon >= 120.0f && lon < 135.0f) && (lat > 30.0f && lat <= 40.0f)) //Korea/Japan
            || ((lon >= 140.0f && lon < 180.0f) && (lat > 60.0f && lat <= 70.0f))) //"Land Bridge" between Russia and Alaska
        {
            land = true;
            region = "Asia";
        }

        //Africa
        else if (((lon >= -15.0f && lon < 35.0f) && (lat > 5.0f && lat <= 35.0f)) //Sahara
            || ((lon >= 35.0f && lon < 40.0f) && (lat > 0.0f && lat <= 5.0f)) //Eastern Africa
            || ((lon >= 40.0f && lon < 52.0f) && (lat > 0.0f && lat <= 10.0f)) //Somalia
            || ((lon >= 10.0f && lon < 40.0f) && (lat > -35.0f && lat <= 5.0f)) //Central/Southern Africa
            || ((lon >= 35.0f && lon < 30.0f) && (lat > -25.0f && lat <= -10.0f))) //Madagascar
        {
            land = true;
            region = "Africa";
        }

        //Australia/New Zealand
        else if (((lon >= 120.0f && lon < 145.0f) && (lat > -20.0f && lat <= -10.0f)) //North Australia
            || ((lon >= 115.0f && lon < 155.0f) && (lat > -30.0f && lat <= -20.0f)) //Central Australia
            || ((lon >= 140.0f && lon < 150.0f) && (lat > -40.0f && lat <= -30.0f)) //Southeast Australia
            ||((lon >= 165.0f && lon < 180.0f) && (lat > -45.0f && lat <= -35.0f))) //New Zealand
        {
            land = true;
            region = "Australia__New_Zealand";
        }

        //Antarctica
        else if (((lon >= -80.0f && lon < -60.0f) && (lat > -70.0f && lat <= -60.0f)) //Tip close to South America
            || ((lon >= -140.0f && lon < 170.0f) && (lat > -90.0f && lat <= -70.0f))) //The rest of Antarctica
        {
            land = true;
            region = "Antarctica";
        }
        else
        {
            land = false;
        }
        
        //Checks to see what ocean the camera is above if it is not above land.

        if (land == false)
        {
            //Pacific Ocean
            if (((lon >= -180.0f && lon < -100.0f) && (lat < 75.0f && lat >= 0.0f) && (land == false))
                || ((lon >= -180.0f && lon < -80.0f) && (lat < 0.0f && lat >= -90.0f) && (land == false))
                || ((lon >= 100.0f && lon < 180.0f) && (lat < 75.0f && lat >= -20.0f) && (land == false))
                || ((lon >= 145.0f && lon < 180.0f) && (lat < -20.0f && lat >= -90.0f) && (land == false)))
            {
                region = "Pacific";
            }

            //Atlantic Ocean
            else if (((lon >= -100.0f && lon < 20.0f) && (lat < 75.0f && lat >= 20.0f) && (land == false))
                || ((lon >= -80.0f && lon < 20.0f) && (lat < 20.0f && lat >= -90.0f) && (land == false)))
            {
                region = "Atlantic";
            }

            //Indian Ocean
            else if (((lon >= 20.0f && lon < 100.0f) && (lat < 75.0f && lat >= -20.0f) && (land == false))
                || ((lon >= 20.0f && lon < 145.0f) && (lat < -20.0f && lat >= -90.0f) && (land == false)))
            {
                region = "Indian";
            }

            //Arctic Ocean
            else if ((lon >= -180.0f && lon < 180.0f) && (lat >= 75.0f && lat <= 90.0f) && (land == false))
            {
                region = "Arctic";
            }

        }

        //Code block that controls the camera taking photos with every frame

        if (takePictures == true)
        {
            takeHiResShot = true;
            if (takeHiResShot)
            {
                RenderTexture rt = new RenderTexture(resWidth, resHeight, 24);
                Camera cam = gameObject.GetComponent<Camera>();
                cam.targetTexture = rt;
                Texture2D screenShot = new Texture2D(resWidth, resHeight, TextureFormat.RGB24, false);
                cam.Render();
                RenderTexture.active = rt;
                screenShot.ReadPixels(new Rect(0, 0, resWidth, resHeight), 0, 0);
                cam.targetTexture = null;
                RenderTexture.active = null; // JC: added to avoid errors
                Destroy(rt);
                byte[] bytes = screenShot.EncodeToPNG();
                string filename = ScreenShotName(resWidth, resHeight, region);
                System.IO.File.WriteAllBytes(filename, bytes);
                //Debug.Log(string.Format("Took screenshot to: {0}", filename));
                takeHiResShot = false;
            }
        }
        
    }
}
