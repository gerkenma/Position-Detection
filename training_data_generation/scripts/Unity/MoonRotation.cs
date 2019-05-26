using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;
using UnityEngine;

public class MoonRotation : MonoBehaviour {
    //This script governs the behavior of the Moon component in the Unity environment
    //It interpolates between the points described in the MoonData.csv file

    //A tutorial that shows the basic methodology used in implementing interpolation can be found on
    //the Vector3.Lerp page in the Unity Scripting API

    //List of ArrayLists that contains all of the parsed data from the csv file
    List<ArrayList> table = new List<ArrayList>();

    //Boolean variable that keeps track of whether the year specfied in the DateConfig file is "2019"
    private bool istwoZeroOneNine = true;

    //Arrays to keep track of the total days of the year elapsed at the start of each month
    private int[] twoZeroOneNineDaySumsByMonth = { 0, 31, 59, 90, 120, 151, 181, 212, 243, 273, 304, 334 };
    private int[] twoZerotwoZeroDaySumsByMonth = { 0, 31, 60, 91, 121, 152, 182, 213, 244, 274, 305, 335 };

    //ArrayList to take the parsed data from the DateConfig file
    private ArrayList config = new ArrayList();

    //The number of units per second that the object travels in the simulation - can be increased or reduced manually
    private float speed = 20.0f;
    
    //Other variables to assist the script in interpolating between the points detailed in the MoonData.csv file
    private float startTime;
    private float journeyLength;
    private float fracJourney;
    private float distCovered;
    public int index = 0;
    private ArrayList startpoint;
    private ArrayList endpoint;
    private Vector3 startposition;
    private Vector3 endposition;

    //Scale factor to keep the Moon orbiting around the Earth at a 90 unit radius in Unity,
    //instead of at an astronomically high radius
    private float scalefactor = 19700000f;

    public static T DeepClone<T>(T obj)
    {
        using (var stream = new MemoryStream())
        {
            var formatter = new BinaryFormatter();
            formatter.Serialize(stream, obj);
            stream.Position = 0;
            return (T)formatter.Deserialize(stream);
        }
    }

    // Use this for initialization
    void Start()
    {

        //Code block that reads from the MoonData.csv file and loads the rows into the table variable
        using (var reader = new StreamReader("MoonData.csv"))
        {
            string line;
            string[] values;
            ArrayList alist = new ArrayList();

            while (!reader.EndOfStream)
            {
                line = reader.ReadLine();
                values = line.Split(',');
                alist.Add(Convert.ToInt32(values[2]));
                alist.Add(Convert.ToInt32(values[3]));
                alist.Add(Convert.ToInt32(values[4]));
                alist.Add(Convert.ToInt32(values[5]));
                alist.Add(Convert.ToInt32(values[6]));
                alist.Add(Convert.ToInt32(values[7]));
                alist.Add(float.Parse(values[8], System.Globalization.NumberStyles.Float));
                alist.Add(float.Parse(values[9], System.Globalization.NumberStyles.Float));
                alist.Add(float.Parse(values[10], System.Globalization.NumberStyles.Float));
                table.Add(DeepClone(alist));
                alist.Clear();
            }
        }

        //Instantation of variables to assist in interpolation
        startTime = Time.time;
        startpoint = table[index];
        endpoint = table[index + 1];

        //The points in the Unity environment that the Moon should interpolate between
        //The points need to be converted from a space dynamics coordinate system to the Unity coordinate system
        //Space dynamics coordinates: (x, y, z)
        // |
        // V
        //Unity coordinates: (-y, z, x)
        //x, y, and z in space dynamics coordinates are rearranged in unity, so that
        //the y coordinate in space dynamics is made negative and is the new x coordinate in Unity
        //the x coordinate in space dynamics is made into the new z coordinate in Unity
        //the z coordinate in space dynamics is made into the new y coordinate in Unity
        startposition = new Vector3(-((float)startpoint[7] / scalefactor), ((float)startpoint[8] / scalefactor), ((float)startpoint[6] / scalefactor));
        endposition = new Vector3(-((float)endpoint[7] / scalefactor), ((float)endpoint[8] / scalefactor), ((float)endpoint[6] / scalefactor));

        journeyLength = Vector3.Distance(startposition, endposition);

        //Debug.Log(journeyLength);

        //Code block to read from the DateConfig file
        using (var reader2 = new StreamReader("DateConfig.txt"))
        {
            string line;
            string[] values;

            while (!reader2.EndOfStream)
            {
                line = reader2.ReadLine();
                values = line.Split(' ');
                config.Add(values[1]);
            }
        }

        //Checks to see if the year is 2019 or not. If it's not, then the simulation starts reading from the 8761st row
        //in the csv file
        if ((string)config[0] == "2019")
        {

        }
        else
        {
            index += 8760;
            istwoZeroOneNine = false;
        }

        int month = Convert.ToInt32(config[1]);
        int day = Convert.ToInt32(config[2]);
        int hour = Convert.ToInt32(config[3]);

        //Code to figure out what row number in the csv file the simulation starts at
        //For every month, 24 * the number of days in that month is added to the starting row number
        //For every day, 24 are added to the starting row number
        //For every hour, 1 is added to the starting row number
        if (istwoZeroOneNine == true)
        {
            index += (twoZeroOneNineDaySumsByMonth[month - 1] * 24) + ((day - 1) * 24) + (hour);
        }
        else
        {
            index += (twoZerotwoZeroDaySumsByMonth[month - 1] * 24) + ((day - 1) * 24) + (hour);
        }

        //Debug.Log(index + 1);
    }

    // Update is called once per frame
    void Update()
    {
        //transform.RotateAround(Vector3.zero, new Vector3(0, 1, 0), 2.5f*Time.deltaTime);

        //If Point B is reached by the directional light, the next point is taken from the csv file, and the directional
        //light component starts interpolating to that point
        if ((transform.position == endposition) && ((index + 1) != (table.Count - 1)))
        {
            index++;

            distCovered = 0;
            fracJourney = 0;
            startTime = Time.time;
            startpoint = table[index];
            endpoint = table[index + 1];

            startposition = new Vector3(-((float)startpoint[7] / scalefactor), ((float)startpoint[8] / scalefactor), ((float)startpoint[6] / scalefactor));
            endposition = new Vector3(-((float)endpoint[7] / scalefactor), ((float)endpoint[8] / scalefactor), ((float)endpoint[6] / scalefactor));

            journeyLength = Vector3.Distance(startposition, endposition);
            //Debug.Log(journeyLength);
        }

        //Code to do the interpolation process
        distCovered = (Time.time - startTime) * speed;

        fracJourney = distCovered / journeyLength;

        transform.position = Vector3.Lerp(startposition, endposition, fracJourney);

        //Ensures that the directional light component continuously looks at the center of the
        //Unity environment - where Earth is
        transform.LookAt(Vector3.zero);
    }
}
