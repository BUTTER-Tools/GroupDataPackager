using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Drawing;
using PluginContracts;
using OutputHelperLib;
using GroupDataObj;
using System.Text;

namespace GroupDataPackager
{
    public class GroupDataPackager : Plugin
    {


        public string[] InputType { get; } = { "String" };
        public string OutputType { get; } = "GroupData";

        public Dictionary<int, string> OutputHeaderData { get; set; } = new Dictionary<int, string>() { { 0, "TokenizedText" } };
        public bool InheritHeader { get; } = false;

        #region Plugin Details and Info

        public string PluginName { get; } = "Group Data Packager";
        public string PluginType { get; } = "Dyads & Groups";
        public string PluginVersion { get; } = "1.0.1";
        public string PluginAuthor { get; } = "Ryan L. Boyd (ryan@ryanboyd.io)";
        public string PluginDescription { get; } = "Takes a set of String data and packages it up as GroupData. Useful for taking separate input texts (e.g., loading multiple columns from a CSV as separate text) and preparing them for analyses like LSM." + Environment.NewLine + Environment.NewLine +
                                                   "Note that packaging up GroupData can be somewhat complicated, depending on the format of your input data. If you want to calculate all pairwise LSM scores within a single text (e.g., you have multiple texts all contained within one text file), you can first " +
                                                   "segment your texts using the \"Segment Text into Chunks\" plugin. In most cases, however, the ideal way to use this plugin is when your texts are contained in a CSV file, with each group of texts on a single row, and each individual text in its own column.";
        public string PluginTutorial { get; } = "https://youtu.be/84TJ-fVjM34";
        public bool TopLevel { get; } = false;


        public Icon GetPluginIcon
        {
            get
            {
                return Properties.Resources.icon;
            }
        }

        #endregion



        public void ChangeSettings()
        {

            MessageBox.Show("This plugin does not have any settings to change.",
                    "No Settings", MessageBoxButtons.OK,
                    MessageBoxIcon.Information);

        }





        public Payload RunPlugin(Payload Input)
        {



            Payload pData = new Payload();
            pData.FileID = Input.FileID;


            //so, the logic right now is that if there are more than one of each segment number,
            //that means that the segments should be paired into a group.
            //if there is only one of each segment, then it should all be treated as part of a single group
            string packageMethod = "All One Group";
            ulong maxSegmentNumber = 0;

            //this is going to store the indices of each segment
            Dictionary<ulong, List<int>> segCount = new Dictionary<ulong, List<int>>();

            for (int i = 0; i < Input.SegmentNumber.Count; i++)
            {
                if (segCount.ContainsKey(Input.SegmentNumber[i]))
                {
                    packageMethod = "Paired Segments";
                    segCount[Input.SegmentNumber[i]].Add(i);
                }
                else
                {
                    segCount.Add(Input.SegmentNumber[i], new List<int> { i });
                    if (Input.SegmentNumber[i] > maxSegmentNumber) maxSegmentNumber = Input.SegmentNumber[i];
                }
            }

            




            //now, we start packing up our groups
            

            if (packageMethod == "All One Group")
            {
                #region All One Group

                GroupData Group = new GroupData();

                for (int i = 0; i < Input.SegmentNumber.Count; i++)
                {
                    List<string> personText = new List<string>() { Input.StringList[i] };
                    Group.People.Add(new Person(Input.SegmentNumber[i].ToString(), personText));
                }

                pData.ObjectList.Add(Group);
                pData.SegmentNumber.Add(1);
                if (Input.SegmentID.Count > 0) pData.SegmentID.Add(Input.SegmentID[0]);
                #endregion

            }
            else if (packageMethod == "Paired Segments")
            {

                #region Paired Segments

                //for (ulong i = 0; i < maxSegmentNumber; i++)
                foreach (ulong key in segCount.Keys)
                {

                    

                    GroupData Group = new GroupData();


                    StringBuilder segmentID = new StringBuilder();

                    for(int j = 0; j < segCount[key].Count; j++)
                    {


                        List<string> personText = new List<string>() { Input.StringList[segCount[key][j]] };

                        if (Input.SegmentID.Count > 0)
                        {

                            Group.People.Add(new Person(Input.SegmentID[segCount[key][j]], personText));
                            segmentID.Append(Input.SegmentID[segCount[key][j]] + ";");
                        }
                        else
                        {
                            Group.People.Add(new Person(j.ToString(), personText));
                            segmentID.Append(j.ToString() + ";");
                        }
       
                    }

                    pData.SegmentNumber.Add(key);
                    pData.SegmentID.Add(segmentID.ToString());
                    pData.ObjectList.Add(Group);



                }

                #endregion



            }


            return (pData);

        }



        public void Initialize() { }

        public bool InspectSettings()
        {
            return true;
        }

        public Payload FinishUp(Payload Input)
        {
            return (Input);
        }



        #region Import/Export Settings
        public void ImportSettings(Dictionary<string, string> SettingsDict)
        {

        }

        public Dictionary<string, string> ExportSettings(bool suppressWarnings)
        {
            Dictionary<string, string> SettingsDict = new Dictionary<string, string>();
            return (SettingsDict);
        }
        #endregion



    }
}
