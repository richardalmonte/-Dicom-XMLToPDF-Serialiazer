/*
 * This Software  and its supporting documentation were developed by 
 *      
 *      Richard Almonte
 *      
 *  As part of the project for internship at Sygest S.R.L, Parma Italy.
 *  Company's  tutor:  Stefano Maestri.  
 *  Academic Tutor: Federico Bergenti
 *  
 * 
 */
using System;
using System.Collections.Generic;
//using System.Linq;
using System.Text;
using System.Configuration;
using System.IO;
using System.Windows.Forms;

namespace DicomXml
{
    /// <summary>
    /// Define the elements and methods that get the dictionary elements into a DicomDictionary Object.
    /// Also define the operations supported by the Dictionary object.
    /// </summary>
    class DicomDictionary
    {
        private string[] elementTag;
        private string[] elementVR;
        private string[] elementDescription;
        public static Dictionary<string, string> myDictionary;
        public static Dictionary<string, string> myImplicitDictionary;

        /// <summary>
        /// Get the entries of the Dictionary element reading line by line.
        /// </summary>
        /// <param name="path"> get the Dicom Dictionary file path</param>
        private void getEntry(string path)
        {
            string[] lines;
            lines = File.ReadAllLines(path);

            string[] readed = new string[5];
            int i = 0;
            //initialize the arrays to a new array with dimensions of file length.
            elementTag = new string[lines.Length];
            elementDescription = new string[lines.Length];
            elementVR = new string[lines.Length];

            foreach (string line in lines)
            {

                readed = parser(line);
                // readed may be null if the read line is not a Dicom Element.
                if (readed != null)
                {
                    elementTag[i] = readed[0];
                    elementVR[i] = readed[2];
                    elementDescription[i] = readed[3];
                    i++;
                }
                // if the parser method result is null the arrays size must be re-dimensioned
                else
                {
                    Array.Resize<string>(ref elementTag, elementDescription.Length - 1);
                    Array.Resize<string>(ref elementDescription, elementDescription.Length - 1);
                    Array.Resize<string>(ref elementVR, elementVR.Length - 1);
                }
            }

        }

        /// <summary>
        /// Parsing of the read it line by getEntry method and eliminates all unnecessary characters. <seealso cref=" getEntry(string)"/>
        /// </summary>
        /// <param name="toParse">get the string to be parsed</param>
        /// <returns>Returns the string without unnecessary characters</returns>
        private string[] parser(string toParse)
        {

            if (!toParse.StartsWith("#"))
            {
                string[] parsed = new string[6];
                char[] delimiters = { '(', ',', ')', '\t' };

                StringBuilder sBuilder = new StringBuilder();

                parsed = toParse.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

                Array.Resize<string>(ref parsed, parsed.Length - 1); // take off the last element of the string that must be the VM of the entry.
                sBuilder.Append(parsed[0]);
                sBuilder.Append(parsed[1]);

                parsed[0] = sBuilder.ToString();

                parsed[1] = string.Empty;

                return parsed;
            }
            else
                return null;
        }

        /// <summary>
        /// Create the dictionary element and populate it. 
        /// </summary>
        public void createDicomDictionary()
        {
            try
            {
				string path = ConfigurationManager.AppSettings["DictionaryPath"];

                getEntry(path/*ConfigurationManager.AppSettings["DictionaryPath"]*/);

                myDictionary = new Dictionary<string, string>();
                myImplicitDictionary = new Dictionary<string, string>();
                for (int i = 0; i < elementTag.Length; i++)
                {
                    if (!myDictionary.ContainsKey(elementTag[i]))
                    {
                        myDictionary.Add(elementTag[i], elementDescription[i]);
                        myImplicitDictionary.Add(elementTag[i], elementVR[i]);
                    }

                }
            }
            catch (FileNotFoundException)
            {

                System.Windows.Forms.MessageBox.Show("Please check your Dictionary file. " + ConfigurationManager.AppSettings["DictionaryPath"], "Error: the dictionary file not Found!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Perfoms a search into myDicomDictionary object for an (group, element) tag and Atribute Name.
        /// </summary>
        /// <param name="key">Dicom element tag as union of DICOM group and element (read from the DICM file) into an unique string</param>
        /// <returns>Returns the result of the search.</returns>
        public string dictLookup(string key)
        {
            string value = string.Empty;
            int tag=0;
            // this data element Tags must not to be converted, are delimiters except for 7FE00010 that is the pixel data element.
            if (key != "7FE00000" && key != "7FE00010" && key != "FFFEE00D" && key != "FFFEE000" && key != "FFFCFFFC" /*"DataSetTrailingPadding"*/ && key != "FFFEE0DD"/*Image Sequence Delimiter*/)
                tag = Convert.ToInt32(key.Substring(0, 4),16);
            if (tag != 0 && tag % 2 != 0 && tag != 0)
                value = "Private Element";
            else if (myDictionary.ContainsKey(key))
                myDictionary.TryGetValue(key, out value);
            else
                value = null;
            return value;
        }
        public string dictLookup(string key, bool impl)
        {
            string value = string.Empty;
            int tag=0;
            if (key != "7FE00000" && key != "7FE00010" && key != "FFFEE00D")
                tag = Convert.ToInt32(key.Substring(0, 4));
            if (tag != 0 && tag % 2 != 0 && tag != 0)
                value = "Private Element";
            else if (myImplicitDictionary.ContainsKey(key))
                myImplicitDictionary.TryGetValue(key, out value);
            else
                value = null;
            return value;
        }
    }
}
