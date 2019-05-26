using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System.Linq;
using TMPro;


public class ConsoleOutput
{
    public void print(object msg)
    {
        Debug.Log(msg);
    }
}
public class Sample : ConsoleOutput
{
    private Vector2 m_startPoint;
    private Vector2 m_endPoint;
    private List<Vector2> m_rawPoints;
    private List<Vector2> m_sampledPoints;
    private List<Vector2> m_normalizedSampledPoints;
    private int m_sampleSize;

    public Sample() { }
    public Sample(List<Vector2> rawPoints, int sampleSize = 100)
    {
        invokeSampling(rawPoints, sampleSize);
    }
    public void invokeSampling(List<Vector2> rawPoints, int sampleSize = 100)
    {
        m_startPoint = rawPoints[0];
        m_endPoint = rawPoints[rawPoints.Count - 1];
        m_rawPoints = rawPoints;
        m_sampleSize = sampleSize;
        m_sampledPoints = new List<Vector2>();
        m_normalizedSampledPoints = new List<Vector2>();
        resamplePoints();
        normalizeSamplePoints();
    }

    //getter setter for 'm_sampledPoints' 
    public List<Vector2> SampledPoints
    {
        get { return m_sampledPoints; }
        set { }
    }
    //getter setter for 'm_normalizedSampledPoints' 
    public List<Vector2> NormalizedSampledPoints
    {
        get { return m_normalizedSampledPoints; }
        set { }
    }
    // process raw points to fixed number of equidistance points
    private void resamplePoints()
    {
        float totalDist = sumDist(); // get the total summed dist of each point in the stroke
        float equiDist = totalDist / m_sampleSize;
        Vector2 beginPoint;

        float sum = 0;
        m_sampledPoints.Add(m_startPoint);
        if (m_rawPoints.Count > 1)
        {
            for (int i = 0; i < m_rawPoints.Count; i++)
            {
                if (sum >= equiDist) //checking the last index's sum result
                {
                    Vector2 addPoint = m_rawPoints[i];
                    if (sum != equiDist)
                    {
                        addPoint = Vector2.Lerp(m_sampledPoints[m_sampledPoints.Count - 1], m_rawPoints[i], equiDist / sum);
                        i--;
                    }
                    m_sampledPoints.Add(addPoint);
                    beginPoint = addPoint;
                    sum = 0;
                }
                else
                    beginPoint = m_rawPoints[i];

                if (i < m_rawPoints.Count - 1)
                    sum += Vector2.Distance(beginPoint, m_rawPoints[i + 1]);
            }
        }

        while (m_sampledPoints.Count < 100)
            m_sampledPoints.Add(m_sampledPoints[m_sampledPoints.Count - 1]);

        while (m_sampledPoints.Count > 100)
            m_sampledPoints.Remove(m_sampledPoints[m_sampledPoints.Count - 1]);

        //print(equiDist + " Sample size : " + m_sampleSize + " | " + m_sampledPoints.Count + " ____________");
    }
    private void normalizeSamplePoints()
    {
        float minX = m_sampledPoints[0].x;
        float minY = m_sampledPoints[0].y;

        float maxX = m_sampledPoints[0].x;
        float maxY = m_sampledPoints[0].y;

        for (int i = 1; i < m_sampledPoints.Count; i++)
        {
            if (minX > m_sampledPoints[i].x)
                minX = m_sampledPoints[i].x;
            else if (maxX < m_sampledPoints[i].x)
                maxX = m_sampledPoints[i].x;

            if (minY > m_sampledPoints[i].y)
                minY = m_sampledPoints[i].y;
            else if (maxY < m_sampledPoints[i].y)
                maxY = m_sampledPoints[i].y;
        }
        float boundingBoxWidth = Math.Abs(maxX - minX);
        float boundingBoxHeight = Math.Abs(maxY - minY);
        float maxLen = Math.Max(boundingBoxWidth, boundingBoxHeight);

        Vector2 minCoord = new Vector2(minX, minY);
        Vector2 maxCoord = new Vector2(maxX, maxY);
        Vector2 centroid = new Vector2((minCoord.x + maxCoord.x) / 2, (minCoord.y + maxCoord.y) / 2);


        for (int i = 0; i < m_sampledPoints.Count; i++)
        {
            float normalized_pointX = m_sampledPoints[0].x;
            float normalized_pointY = m_sampledPoints[0].y;
            if (maxLen != 0)
            {
                normalized_pointX = (m_sampledPoints[i].x - centroid.x) / maxLen;
                normalized_pointY = (m_sampledPoints[i].y - centroid.y) / maxLen;
            }
            m_normalizedSampledPoints.Add(new Vector2(normalized_pointX, normalized_pointY));
        }

    }
    private float sumDist()
    {
        float totalLen = 0;
        for (int i = 0; i < m_rawPoints.Count - 1; i++)
        {
            totalLen += Vector2.Distance(m_rawPoints[i], m_rawPoints[i + 1]);
        }
        return totalLen;
    }
}
public class KeyboardLayout
{
    private readonly Dictionary<string, Vector2> m_qwertyLayout;
    private readonly float m_qwerty_key_spacing_w;
    private readonly float m_qwerty_key_spacing_h;
    private readonly float m_qwertyKeyRadius;

    public KeyboardLayout(bool isQWERTY = true)
    {
        if (isQWERTY)
        {

            float SPACING_PER_KEY_W = 0.25f; // width per key
            float SPACING_PER_KEY_H = 0.4210f; // height per key
            float KEY_RADIUS = Math.Min(SPACING_PER_KEY_W, SPACING_PER_KEY_H);
            Vector2 Q_KEY = new Vector2(-1.125f, 1.9195f);
            //Vector2 Q_KEY = new Vector2(-0.9785f, 1.9f);
            Vector2 A_KEY = new Vector2(Q_KEY.x + 1 * SPACING_PER_KEY_W / 2, Q_KEY.y - 1 * SPACING_PER_KEY_H);
            Vector2 Z_KEY = new Vector2(Q_KEY.x + SPACING_PER_KEY_W + SPACING_PER_KEY_W / 2, Q_KEY.y - 2 * SPACING_PER_KEY_H);
            m_qwertyLayout = new Dictionary<string, Vector2>()
            {
                { "q",Q_KEY}, { "w",new Vector2(Q_KEY.x + 1 * SPACING_PER_KEY_W, Q_KEY.y)}, { "e",new Vector2(Q_KEY.x + 2 * SPACING_PER_KEY_W, Q_KEY.y)},
                { "r",new Vector2(Q_KEY.x + 3 * SPACING_PER_KEY_W, Q_KEY.y)}, { "t",new Vector2(Q_KEY.x + 4 *SPACING_PER_KEY_W, Q_KEY.y)}, { "y",new Vector2(Q_KEY.x + 5* SPACING_PER_KEY_W, Q_KEY.y)},
                { "u",new Vector2(Q_KEY.x + 6 * SPACING_PER_KEY_W, Q_KEY.y)}, { "i",new Vector2(Q_KEY.x + 7 * SPACING_PER_KEY_W, Q_KEY.y)},{ "o",new Vector2(Q_KEY.x + 8 * SPACING_PER_KEY_W, Q_KEY.y)},
                { "p",new Vector2(Q_KEY.x + 9 * SPACING_PER_KEY_W, Q_KEY.y)}, { "a", A_KEY}, { "s",new Vector2(A_KEY.x +  1 * SPACING_PER_KEY_W, A_KEY.y)},
                { "d",new Vector2(A_KEY.x + 2 * SPACING_PER_KEY_W, A_KEY.y)},{ "f",new Vector2(A_KEY.x + 3 * SPACING_PER_KEY_W, A_KEY.y)}, { "g",new Vector2(A_KEY.x + 4 * SPACING_PER_KEY_W, A_KEY.y)},
                { "h",new Vector2(A_KEY.x + 5 * SPACING_PER_KEY_W, A_KEY.y)},{ "j",new Vector2(A_KEY.x + 6 * SPACING_PER_KEY_W, A_KEY.y)}, { "k",new Vector2(A_KEY.x + 7 * SPACING_PER_KEY_W, A_KEY.y)},
                { "l",new Vector2(A_KEY.x + 8 * SPACING_PER_KEY_W, A_KEY.y)},{ "z", Z_KEY}, { "x",new Vector2(Z_KEY.x + 1 * SPACING_PER_KEY_W, Z_KEY.y)},
                { "c",new Vector2(Z_KEY.x + 2 * SPACING_PER_KEY_W, Z_KEY.y)},{ "v",new Vector2(Z_KEY.x + 3 * SPACING_PER_KEY_W, Z_KEY.y)}, { "b",new Vector2(Z_KEY.x + 4 * SPACING_PER_KEY_W, Z_KEY.y)},
                { "n",new Vector2(Z_KEY.x + 5 * SPACING_PER_KEY_W, Z_KEY.y)},{ "m",new Vector2(Z_KEY.x + 6 * SPACING_PER_KEY_W, Z_KEY.y)}
            };

            m_qwerty_key_spacing_w = SPACING_PER_KEY_W;
            m_qwerty_key_spacing_h = SPACING_PER_KEY_H;
            m_qwertyKeyRadius = KEY_RADIUS;
        }
    }

    public Dictionary<string, Vector2> QwertyLayout
    {
        get { return m_qwertyLayout; }
    }

    public Vector2 getPoint(string keyNames)
    {
        return m_qwertyLayout[keyNames];
    }
    public float QwertyKeyRadius
    {
        get { return m_qwertyKeyRadius; }
    }
    public float Qwerty_key_spacing_w
    {
        get { return m_qwerty_key_spacing_w; }
    }
    public float Qwerty_key_spacing_h
    {
        get { return m_qwerty_key_spacing_h; }
    }

}
public class WordTemplate : ConsoleOutput
{
    private string m_word;
    private List<Vector2> m_initialPoints;
    private List<Vector2> m_sampledPoints;
    private List<Vector2> m_normalizedSampledPoints;
    private KeyboardLayout m_keyboardLayout;
    private Sample m_sampler;

    public WordTemplate(string word)
    {
        m_word = word.ToLower();
        m_keyboardLayout = new KeyboardLayout(); //Qwerty keyboard layout
        m_initialPoints = new List<Vector2>();
        generateTemplate();
    }
    public string Word
    {
        get { return m_word; }
    }
    public List<Vector2> SampledPoints
    {
        get { return m_sampledPoints; }
    }
    public List<Vector2> NormalizedSampledPoints
    {
        get { return m_normalizedSampledPoints; }
    }
    private void generateTemplate()
    {
        string prevWord = "";
        string curWord = "";

        //getRawPoints
        for (int i = 0; i < m_word.Length; i++)
        {
            curWord = m_word[i].ToString();
            ////print(m_word[i].ToString() + "_" + m_keyboardLayout.getPoint(m_word[i].ToString()));
            if (prevWord != curWord)
                m_initialPoints.Add(m_keyboardLayout.getPoint(curWord));
            prevWord = curWord;
            //m_initialPoints.Add(m_keyboardLayout.getPoint(curWord));
        }
        //getsampledPoints
        m_sampler = new Sample(m_initialPoints, 100);
        m_sampledPoints = m_sampler.SampledPoints;
        m_normalizedSampledPoints = m_sampler.NormalizedSampledPoints;
    }
}
public class Analysis : ConsoleOutput
{
    private KeyboardLayout KeyboardLayout;
    private float m_shapeMatchingScore;
    private float m_LocationMatchingScore;
    public Analysis()
    {
        KeyboardLayout = new KeyboardLayout();
    }
    public Analysis(List<Vector2> userInput, List<Vector2> template)
    {
        KeyboardLayout = new KeyboardLayout();
        m_shapeMatchingScore = shapeAnalysis(userInput, template);
        m_LocationMatchingScore = locationAnalysis(userInput, template);
    }
    public float shapeAnalysis(List<Vector2> userInput, List<Vector2> template)
    {
        if (userInput.Count != template.Count)
        {
            print("Something went wrong, the number of sample points differ");
            return -1;
        }

        //linearMatching
        float linearDistSum = 0;
        for (int i = 0; i < userInput.Count; i++)
        {
            linearDistSum += Vector2.Distance(userInput[i], template[i]);
        }


        //print(linearDistSum / userInput.Count);
        return linearDistSum;
        //return (1.0f - linearDistSum / userInput.Count);
    }
    public float locationAnalysis(List<Vector2> userInput, List<Vector2> template)
    {
        if (userInput.Count != template.Count)
        {
            print("Something went wrong, the number of sample points differ");
            return -1;
        }

        float keyRadius = KeyboardLayout.QwertyKeyRadius;
        //bool checkPerfectScore = true;
        bool checkPerfectScore = false;
        float weight = 0.0f;
        float weightSum = 0;
        float totalSumDist = 0;

        //int midPointIndex = (int)Math.Floor(userInput.Count / 2.0f);
        for (int i = 0; i < userInput.Count; i++) //p
        {
            if (checkPerfectScore)
            {
                ////get the key with min distance (p,q)
                //float minDist = 999999.0f;
                //for (int j = 0; j < template.Count; j++) //q
                //{
                //    float dist = Vector2.Distance(userInput[i], template[j]);
                //    if (minDist > dist)
                //    {
                //        minDist = dist;
                //    }
                //}
                ////check if the input is inside the invisible 'tunnel'
                //float distFromKeyCentroid = Math.Max(minDist - keyRadius, 0);
                //if (distFromKeyCentroid != 0)
                //{
                //    checkPerfectScore = false;
                //}
            }
            //linearMatching
            float linearDist = Vector2.Distance(userInput[i], template[i]);
            weight = calcWeight(i / userInput.Count);
            //weight = calcWeight(i * 2 / userInput.Count);
            //if (i < midPointIndex)
            //    weight = calcWeight(i * 2 / userInput.Count, true);
            //else
            //    weight = calcWeight(i * 2 / userInput.Count, false);
            weightSum += weight;
            totalSumDist += weight * linearDist;
        }
        //print("totalSumDist : " + totalSumDist);

        //lower the better
        //print(totalSumDist / userInput.Count);
        if (checkPerfectScore)
        {
            print("PERFECT");
            return 0;
        }
        else
            return totalSumDist / weightSum; //to make the total summed value to 1
    }
    // y = -x + 1 : linear decrease function (x: 0~1, y:1~0)
    // y = +x - 1 : linear increase function (x: 1~2, y:0~1)
    // i: x, weight: y
    private float calcWeight(int x, bool isDecreaseFunc = true) //y = a(x-0.5)^2 + min_val
    {
        //float max_val = 0.8f;
        //float min_val = 0.4f;
        //float max_val = 0.9f;
        //float min_val = 0.4f;
        float max_val = MAX;
        float min_val = MIN;
        //float max_val_end = 0.8f;
        float slope = (max_val - min_val) / 0.25f;
        //float slope_increasingSlope = (max_val_end - min_val) / 0.25f;
        float res = (float)(slope * Math.Pow((x - 0.5f), 2) + min_val);
        //float res_end = (float)(slope_increasingSlope * Math.Pow((x - 0.5f), 2) + min_val);

        //if (isDecreaseFunc)
        //    return res;
        //else
        //    return res_end;

        //if (isDecreaseFunc)
        //    return -0.4f * x + 0.8f;
        //else
        //    return +0.4f * x;

        return res;
    }
    //public static float MAX = 0.90f;
    //public static float MIN = 0.53f;
    //public static float MAX = 0.8f;
    //public static float MIN = 0.4f;
    public static float MAX = 0.821188f;
    public static float MIN = 0.466728f;
}
public class AnalysisManager : ConsoleOutput
{
    private List<WordTemplate> templateList;
    private Analysis analyzer;
    private Sample sampler;
    public AnalysisManager()
    {
        analyzer = new Analysis();
        sampler = new Sample();
        templateList = new List<WordTemplate>();

        ////string dataSource = "Assets/Resources/corpus_7899.txt";
        ////string dataSource = "Assets/Resources/corpus_test.txt";
        //string dataSource = "Assets/Resources/corpus_test2.txt";
        string dataSource = "Assets/Resources/corpus_5680.txt";

        StreamReader reader = new StreamReader(dataSource);
        while (!reader.EndOfStream)
        {
            string word = reader.ReadLine();
            word = word.Trim();
            word = word.Trim('\n');
            word = word.Trim('\t');
            templateList.Add(new WordTemplate(word));
        }
        reader.Close();

        templateList.Add(new WordTemplate("qwerty"));
        //templateList.Add(new WordTemplate("kitty"));
        //templateList.Add(new WordTemplate("lorry"));
        //templateList.Add(new WordTemplate("hello"));
        //templateList.Add(new WordTemplate("qwerty"));
        //templateList.Add(new WordTemplate("bye"));
        //templateList.Add(new WordTemplate("mine"));
        //templateList.Add(new WordTemplate("growth"));
        //templateList = new List<WordTemplate>();
        print("Corpus intialization complete");
    }
    public static float ALPHA_FACTOR = 0.5f;
    public static float INTERPOLATE_FACTOR = 0.1f;
    public Dictionary<string, float> analyzeInput(List<Vector2> userRawInput, int sampleSize = 100, bool saveData = false)
    {
        float closestScore_TOTAL = -1;
        int closestScoreIndex_TOTAL = -1;
        float closestScore_shape = 9999999.0f;
        int closestScoreIndex_shape = -1;
        float closestScore_location = 9999999.0f;
        int closestScoreIndex_location = -1;
        ////
        float closestScore_location2 = 9999999.0f;
        int closestScoreIndex_location2 = -1;
        ///
        Dictionary<string, float> closestShapeList = new Dictionary<string, float>();
        Dictionary<string, float> closestLocationList = new Dictionary<string, float>();
        Dictionary<string, float> closestLocationList2 = new Dictionary<string, float>();
        Dictionary<string, float> closestFINALList = new Dictionary<string, float>();
        List<string> scoreList = new List<string>();

        sampler.invokeSampling(userRawInput, sampleSize);
        List<Vector2> normalizedUserInput = sampler.NormalizedSampledPoints;
        List<Vector2> unnormalizedUserInput = sampler.SampledPoints;
        //if (saveData)
        //{
        //    saveToFile("Assets/Resources/normalized_input.csv", normalizedUserInput);
        //    saveToFile("Assets/Resources/unnormalized_input.csv", unnormalizedUserInput, false);
        //}

        for (int i = 0; i < templateList.Count; i++)
        {
            List<Vector2> normalizedtemplate = templateList[i].NormalizedSampledPoints;
            List<Vector2> unnormalizedTemplate = templateList[i].SampledPoints;

            //////
            //List<Vector2> adjustedUserPoints = new List<Vector2>();
            //for (int j = 0; j < unnormalizedTemplate.Count; j++) //ADJUSTING LOCATION CHANNEL
            //{
            //    //adjustedUserPoints.Add(Vector2.Lerp(unnormalizedTemplate[j], unnormalizedUserInput[j], INTERPOLATE_FACTOR)); //big t_val : close to userInput
            //    adjustedUserPoints.Add(Vector2.Lerp(unnormalizedTemplate[j], unnormalizedUserInput[j], 0.4f)); //big t_val : close to userInput
            //    //adjustedUserPoints.Add(Vector2.Lerp(unnormalizedTemplate[j], unnormalizedUserInput[j], 0.3f)); //big t_val : close to userInput
            //}
            //////
            float shape = analyzer.shapeAnalysis(normalizedUserInput, normalizedtemplate);
            //float location = analyzer.locationAnalysis(adjustedUserPoints, unnormalizedTemplate);
            float location = analyzer.locationAnalysis(unnormalizedUserInput, unnormalizedTemplate);
            //float location2 = analyzer.locationAnalysis(adjustedUserPoints, unnormalizedTemplate);

            float weightedShape = ALPHA_FACTOR * (1.0f - shape / sampleSize);

            if (shape > 100.0f) // one syllable word e.g.) A, B, C
                weightedShape = ALPHA_FACTOR;//full score (letting only the location channel to decide)

            float weightedLocation = (1.0f - ALPHA_FACTOR) * (1.0f - location);
            //float weightedLocation2 = (1.0f - ALPHA_FACTOR) * (1.0f - location2);
            //float weightedLocation = (1.0f - ALPHA_FACTOR) * (1.0f - location / sampleSize);
            //float weightedLocation2 = (1.0f - ALPHA_FACTOR) * (1.0f - location2 / sampleSize);
            float score_TOTAL = (weightedShape + weightedLocation) * 100.0f;
            //float score_TOTAL2 = (weightedShape + weightedLocation2) * 100.0f;

            //float score_TOTAL = 100.0f * (1.0f - shape / sampleSize) * (1.0f - location / sampleSize);
            //float weightedShape = ALPHA_FACTOR * (1.0f - shape / sampleSize);
            //float weightedLocation = (1.0f - ALPHA_FACTOR) * (1.0f - location / sampleSize);
            //float score_TOTAL = weightedShape + weightedLocation;

            //do this after finding the optimal alpha value
            //alpha value is the 0.76 above
            //if (closestScore_TOTAL < score_TOTAL)
            //{
            //    //print((1.0f - shape / sampleSize) + "__" + (1.0f - location / sampleSize) + ":::" + score_TOTAL + ":::" + score_TOTAL2);
            //    closestScore_TOTAL = score_TOTAL;
            //    closestScoreIndex_TOTAL = i;
            //}
            if (score_TOTAL > 70)
            {
                if (!closestFINALList.ContainsKey(templateList[i].Word))
                    closestFINALList.Add(templateList[i].Word, score_TOTAL);
            }
        }
        //////
        ////if (closestScore_TOTAL < 95)
        //if (closestScore_TOTAL < 100)
        //{

        if (closestFINALList.Count <= 0)
            return null;

        //var items = from pair in closestFINALList
        //            orderby pair.Value descending
        //            select pair;
        //int cntItem = 0;
        //foreach (KeyValuePair<string, float> pair in items)
        //{
        //    if (cntItem >= 5)
        //        break;
        //    //print(pair.Key + " : " + pair.Value);
        //    //scoreList.Add(pair.Key);
        //    cntItem++;
        //}
        //print("______");


        //}
        //else
        //    print(templateList[closestScoreIndex_TOTAL].Word + " : " + closestScore_TOTAL);
        //print("__________");
        ////
        //print(templateList[closestScoreIndex_TOTAL].Word + " : " + closestScore_TOTAL);

        //return scoreList;
        return closestFINALList;
    }
    //public void saveToFile(string path, List<Vector2> strokePointsList, bool updateIndex = true)
    //{
    //    string keepText = "# of raw,1";
    //    string userInputNum = "1";
    //    string configPath = "Assets/Resources/config.csv";
    //    FileInfo fi = new FileInfo(configPath);
    //    if (fi.Exists)
    //    {
    //        using (var file = File.OpenText(configPath))
    //        {
    //            string[] lines = File.ReadAllLines(configPath);
    //            if (lines != null)
    //            {
    //                userInputNum = (Int32.Parse((lines[1].Split(','))[1]) + 1).ToString();
    //                keepText = lines[0];
    //            }
    //        }
    //        File.Delete(configPath);
    //    }
    //    using (var file = File.CreateText(configPath))
    //    {
    //        if (!updateIndex)
    //            userInputNum = (Int32.Parse(userInputNum) - 1).ToString();

    //        file.WriteLine(keepText);
    //        file.WriteLine("# of processed," + userInputNum);
    //    }

    //    using (var file = File.AppendText(path))
    //    {
    //        file.WriteLine("[USER" + userInputNum + "]");
    //        foreach (var point in strokePointsList)
    //        {
    //            file.WriteLine(point.x + "," + point.y);
    //        }
    //    }
    //}
}
public class DataManager : ConsoleOutput // each data by category
{
    private Dictionary<string, List<Data>> dataset;
    private string label;
    private int numCnt = 0;
    private List<Data> data;
    private readonly string ROOT_DIR = "Assets/Resources/Dataset/";
    private readonly string CSV = ".csv";
    public DataManager()
    {
        dataset = new Dictionary<string, List<Data>>();
        if (!Directory.Exists(ROOT_DIR))
        {
            Directory.CreateDirectory(ROOT_DIR);
        }
        else //synchronize(load) existing data
        {
            DirectoryInfo[] dir = new DirectoryInfo(ROOT_DIR).GetDirectories();
            //FileInfo[] dir = new DirectoryInfo(ROOT_DIR).GetFiles();
            if (dir != null)
            {
                for (int i = 0; i < dir.Length; i++)
                {
                    if (dir[i].Extension == ".meta")
                        continue;
                    string category = dir[i].Name;
                    string categoryDir = ROOT_DIR + category + "/";
                    FileInfo[] allFiles = new DirectoryInfo(categoryDir).GetFiles();
                    foreach (FileInfo file in allFiles)// all the files in a category
                    {
                        if (file.Extension == ".meta")
                            continue;
                        string fileName = file.Name;
                        string fileDir = categoryDir + fileName;
                        List<Vector2> dataPoints = new List<Vector2>();
                        using (var f = File.OpenText(fileDir)) //read one file
                        {
                            string[] lines = File.ReadAllLines(fileDir);
                            if (lines != null)
                            {
                                foreach (var line in lines) //per line (one point)
                                {
                                    string[] splitData = line.Split(',');
                                    dataPoints.Add(new Vector2(float.Parse(splitData[0]), float.Parse(splitData[1])));
                                }
                            }
                        }
                        Data d = new Data(dataPoints);
                        if (dataset.ContainsKey(category))
                        {
                            dataset[category].Add(d);
                        }
                        else //Data's category entering for the first time 
                        {
                            List<Data> dataList = new List<Data>(); //list of strokes
                            dataList.Add(d);
                            dataset.Add(category, dataList);
                        }
                    }
                }
            }
        }
    }
    public void addData(string category, List<Vector2> dataPoints)
    {
        Data data = new Data(dataPoints); //all the data points in a whole stroke
        if (dataset.ContainsKey(category))
        {
            dataset[category].Add(data);
        }
        else //Data's category entering for the first time 
        {
            //into a file(Create parent folder)
            createCategoryDir(category);

            //into a software variable
            List<Data> dataList = new List<Data>(); //list of strokes
            dataList.Add(data);
            dataset.Add(category, dataList);
        }

        //into a file
        createFile(ROOT_DIR + category + "/" + (dataset[category].Count) + CSV, dataPoints);
    }
    public void removeData(string category)
    {
        List<Data> targetItem = dataset[category];
        if (targetItem.Count <= 0)
            return;
        int lastIndex = targetItem.Count - 1;
        string deleteDir = ROOT_DIR + category + "/" + (lastIndex + 1) + CSV;
        dataset[category].Remove(targetItem[lastIndex]); //remove the most recent data
        File.Delete(deleteDir);
        print("removed");
    }
    public int getDataCount(string category)
    {
        return dataset[category].Count;
    }
    public DirectoryInfo[] getCategoryList()
    {
        DirectoryInfo[] list = new DirectoryInfo(ROOT_DIR).GetDirectories();
        return list;
    }
    public List<Vector2> getData(string category, int index)
    {
        return dataset[category][index].DataPoints;
    }
    private void createCategoryDir(string category)
    {
        string fileSaveDir = ROOT_DIR + category + "/";
        if (!Directory.Exists(fileSaveDir))
        {
            Directory.CreateDirectory(fileSaveDir);
        }
    }
    private void createFile(string path, List<Vector2> points)
    {
        using (var file = File.CreateText(path))
        {
            foreach (var point in points)
            {
                file.WriteLine(point.x + "," + point.y);
            }
        }
    }
}
public class Data // one stroke's data points
{
    private readonly List<Vector2> data;
    public Data(List<Vector2> data)
    {
        this.data = data;
    }
    public List<Vector2> DataPoints
    {
        get { return this.data; }
    }
}
public class ScreenManager
{
    private readonly List<string> screenText;
    public ScreenManager()
    {
        screenText = new List<string>()
        {
            "The grass is always greener on the other side",
            "Every cloud has a silver lining",
            "Do not judge a book by its cover",
            "Strike while the iron is hot"
        };
    }
    public int getScreenTextCount()
    {
        return screenText.Count;
    }
    public string getScreenText(int index)
    {
        if (index >= screenText.Count)
            return "";

        return screenText[index];
    }
    public string[] getScreenTextByWord(int index)
    {
        if (index >= screenText.Count)
            return null;

        return screenText[index].Split(' ');
    }
}
public class StrokeRenderer : MonoBehaviour
{
    private readonly string curRecordLabel = ""; //manual category label
    public Camera eye;
    public GameObject cursor; //index finger
    public GameObject swipeDetectectionCursorThumb; //thumb
    public GameObject[] swipeDetectectionCursorEtc; //all fingers except thumb

    public GameObject userInputRender;
    public GameObject templateRender;
    public GameObject adjustedInputRender;
    public GameObject projectionPlane;
    public GameObject qwertyLayout;
    public GameObject datasetCollectionScreenGobj;
    public GameObject[] monitorScreenGobj;

    private LineRenderer lineRender;
    private bool trackGesture;
    private List<Vector2> strokePointsList;//localspace
    private List<Vector3> strokePointsRenderList; // Worldspace
    private Vector3 strokePoint;
    private float renderDepth;
    private AnalysisManager analysisManager;
    private DataManager dataManager;
    private ScreenManager screenManager;
    public NetworkClient network;
    private List<Vector2> saveList;
    private List<string> analyzedResult;
    private List<float> analyzedResult_value;
    private List<string> analyzedResult_copy;
    private Dictionary<int, float> MID_ALPHA_FACTOR = new Dictionary<int, float>();
    private Dictionary<int, float> MID_HIGHEST_SCORE = new Dictionary<int, float>();
    private static float FINAL_ALPHA_FACTOR = 0;
    private static float FINAL_HIGHEST_SCORE = 0;

    public TextMeshProUGUI text_UI;
    public TextMeshProUGUI inputText_UI;
    public TextMeshProUGUI inputText_UI2;
    public GameObject[] suggestion_UI_Gobj;
    public TextMeshProUGUI[] suggestion_UI;//5
    public TextMeshProUGUI[] suggestion_UI_4;//4
    public TextMeshProUGUI[] suggestion_UI_3;//3
    public TextMeshProUGUI[] suggestion_UI_2;//2
    public TextMeshProUGUI[] suggestion_UI_1;//1

    public TextMeshProUGUI currentPageNum_UI;
    public TextMeshProUGUI fullPageNum_UI;
    private int textCurPageNum = 1;
    private int textFullPageNum = 1;
    private string[] curTextList;
    private int textIndex = 0;
    private string prevText = "";
    private bool isPrevTextWrong = false;
    public static string currentGazedObjectHitPoint = "";
    void Awake()
    {
        this.transform.localPosition = Vector3.zero;
        lineRender = this.GetComponent<LineRenderer>();
        setState(lineRender, false);
        trackGesture = false;
        strokePointsList = new List<Vector2>();
        analyzedResult = new List<string>();
        analyzedResult_value = new List<float>();
        analyzedResult_copy = new List<string>();
        strokePointsRenderList = new List<Vector3>();
        renderDepth = projectionPlane.transform.localPosition.z;
        strokePoint = Vector3.zero;
        analysisManager = new AnalysisManager();
        dataManager = new DataManager();
        screenManager = new ScreenManager();
        saveList = new List<Vector2>();
        textFullPageNum = screenManager.getScreenTextCount();
        currentPageNum_UI.text = textCurPageNum.ToString();
        fullPageNum_UI.text = textFullPageNum.ToString();
        text_UI.text = screenManager.getScreenText(textCurPageNum - 1);
        foreach (TextMeshProUGUI txt in suggestion_UI) //initialize (empty)
            txt.text = "";
        foreach (GameObject gobj in suggestion_UI_Gobj) //initialize (empty)
            gobj.SetActive(false);
        //foreach (GameObject gobj in monitorScreenGobj) //initialize (empty)
        //    gobj.SetActive(false);

        inputText_UI.text = "";
        inputText_UI2.text = "";
        curTextList = screenManager.getScreenTextByWord(textCurPageNum - 1);
        //int num = dataManager.getDataCount("kitty");
        //List<Vector2> prevList = dataManager.getData("kitty", num-1);
        //renderStrokePoints(prevList, userInputRender);

        //WordTemplate test_OPEN = new WordTemplate("q");
        //renderStrokePoints(test_OPEN.SampledPoints, templateRender);
        //renderStrokePoints(test_OPEN.SampledPoints, userInputRender);
        qwertyLayout.SetActive(true);
        //StartCoroutine(optimizer(1, 0.00f, 0.025f, true));
        //StartCoroutine(optimizer(2, 0.025f, 0.05f, false));
        //StartCoroutine(optimizer(3, 0.05f, 0.075f, false));
        //StartCoroutine(optimizer(4, 0.075f, 0.1f, false));
        //StartCoroutine(optimizer(5, 0.1f, 0.125f, false));
        //StartCoroutine(optimizer(6, 0.125f, 0.150f, false));
        //StartCoroutine(optimizer(7, 0.15f, 0.175f, false));
        //StartCoroutine(optimizer(8, 0.175f, 0.2f, false));
        //StartCoroutine(optimizer(9, 0.2f, 0.225f, false));
        //StartCoroutine(optimizer(10, 0.225f, 0.25f, false));

        //StartCoroutine(optimizer(11, 0.25f, 0.275f, false));
        //StartCoroutine(optimizer(12, 0.275f, 0.3f, false));
        //StartCoroutine(optimizer(13, 0.3f, 0.325f, false));
        //StartCoroutine(optimizer(14, 0.325f, 0.35f, false));
        //StartCoroutine(optimizer(15, 0.35f, 0.375f, false));
        //StartCoroutine(optimizer(16, 0.375f, 0.4f, false));
        //StartCoroutine(optimizer(17, 0.4f, 0.425f, false));
        //StartCoroutine(optimizer(18, 0.425f, 0.45f, false));
        //StartCoroutine(optimizer(19, 0.45f, 0.475f, false));
        //StartCoroutine(optimizer(20, 0.475f, 0.50f, false));


        //StartCoroutine(optimizer(21, 0.50f, 0.55f, false));
        //StartCoroutine(optimizer(22, 0.55f, 0.60f, false));
        //StartCoroutine(optimizer(23, 0.60f, 0.65f, false));
        //StartCoroutine(optimizer(24, 0.65f, 0.70f, false));
        //StartCoroutine(optimizer(25, 0.70f, 0.75f, false));
        //StartCoroutine(optimizer(26, 0.75f, 0.80f, false));
        //StartCoroutine(optimizer(27, 0.80f, 0.85f, false));
        //StartCoroutine(optimizer(28, 0.85f, 0.90f, false));
        //StartCoroutine(optimizer(29, 0.90f, 0.95f, false));
        //StartCoroutine(optimizer(30, 0.95f, 1.00f, false));
    }
    private void updateSuggestion(List<string> suggestions)
    {
        int index = 0 + 1; //because the highest score is in the "Input Text UI"
        TextMeshProUGUI[] suggestionUI_obj;

        foreach (GameObject gobj in suggestion_UI_Gobj)
            gobj.SetActive(false);

        if (suggestions.Count > 1)
            suggestion_UI_Gobj[suggestions.Count - 2].SetActive(true);

        if (suggestions.Count == 6)
            suggestionUI_obj = suggestion_UI;
        else if (suggestions.Count == 5)
            suggestionUI_obj = suggestion_UI_4;
        else if (suggestions.Count == 4)
            suggestionUI_obj = suggestion_UI_3;
        else if (suggestions.Count == 3)
            suggestionUI_obj = suggestion_UI_2;
        else if (suggestions.Count == 2)
            suggestionUI_obj = suggestion_UI_1;
        else
            return;

        foreach (TextMeshProUGUI txt in suggestionUI_obj) //make it empty
        {
            if (index >= suggestions.Count)
                txt.text = "";
            else
                txt.text = suggestions[index];
            index++;
        }
    }
    private void updateText()
    {
        text_UI.text = screenManager.getScreenText(textCurPageNum - 1);
    }
    private void updatePageNum()
    {
        if (textCurPageNum <= textFullPageNum)
        {
            currentPageNum_UI.text = textCurPageNum.ToString();
        }
    }
    private void setState(GameObject obj, bool state)
    {
        obj.SetActive(state);
    }
    private void setState(TrailRenderer obj, bool state)
    {
        obj.enabled = state;
    }
    private void setState(LineRenderer obj, bool state)
    {
        obj.enabled = state;
    }
    private Vector3 shootRay()
    {
        Vector3 direction = cursor.transform.position - eye.transform.position;

        RaycastHit hit;
        Debug.DrawRay(eye.transform.position, direction * 100.0f, Color.yellow);
        if (Physics.Raycast(eye.transform.position, direction, out hit, Mathf.Infinity))
        {
            //Debug.DrawRay(eye.transform.position, direction * 100.0f, Color.yellow);
            //print(hit.point);
            return hit.point;
        }
        else
            return Vector3.zero;
    }
    public static int gazedKey = -1;
    private int getPressedSuggesionGobj()
    {
        int pressedButton = -1;
        //if (isPrevTextWrong)
        //{
        if (gazedKey != -1)
        {
            pressedButton = gazedKey;
            gazedKey = -1;
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Joystick1Button3))//top most button
            {
                pressedButton = 1;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Joystick1Button1))//right most button
            {
                pressedButton = 2;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Joystick1Button0))//bottom most button)
            {
                pressedButton = 3;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                pressedButton = 4;
            }
            else if (Input.GetKeyDown(KeyCode.Alpha5))
            {
                pressedButton = 5;
            }
        }
        //}

        return pressedButton;
    }
    void Update()
    {
        //swipeDetect();
        int suggestionButtonPressedNum = getPressedSuggesionGobj();

        //left button
        if (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.Joystick1Button2)) // trackGesture ON
        {
            inputText_UI2.text = "";
            setState(lineRender, false);
            trackGesture = true;
        }

        else if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) // Erase previous strokes
        {
            setState(lineRender, false);
            qwertyLayout.SetActive(!qwertyLayout.activeSelf);
        }
        else if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Delete) || Input.GetKey(KeyCode.Escape))// (DELETE) : Erase word
        {
            if (datasetCollectionScreenGobj.activeSelf)
            {
                inputText_UI.text = prevText;
                isPrevTextWrong = false;
            }
            else //delete
            {
                network.sendData("", "delete");
            }
        }
        //else if (Input.GetKeyDown(KeyCode.Escape))// no datacollection screen
        //{
        //    datasetCollectionScreenGobj.SetActive(!datasetCollectionScreenGobj.activeSelf);
        //    foreach (GameObject gobj in monitorScreenGobj) //initialize (empty)
        //        gobj.SetActive(!gobj.activeSelf);
        //}
        else if (suggestionButtonPressedNum != -1) // select suggestion
        {
            if (datasetCollectionScreenGobj.activeSelf)
            {
                if (isPrevTextWrong)
                {
                    if (suggestionButtonPressedNum <= analyzedResult_copy.Count - 1)
                    {
                        string selectedSuggestionWord = analyzedResult_copy[suggestionButtonPressedNum];
                        if (curTextList[textIndex].ToLower() == selectedSuggestionWord.ToLower())
                        {
                            textIndex++;
                            isPrevTextWrong = false;
                            inputText_UI.text = prevText + curTextList[textIndex - 1];
                            prevText = inputText_UI.text;

                            if (textIndex >= curTextList.Length) //last word in the sentence
                            {
                                prevText = "";
                                textIndex = 0;
                                textCurPageNum++;
                                updatePageNum();
                                curTextList = screenManager.getScreenTextByWord(textCurPageNum - 1);
                                text_UI.text = screenManager.getScreenText(textCurPageNum - 1);
                                inputText_UI.text = "";
                                foreach (GameObject gobj in suggestion_UI_Gobj)
                                    gobj.SetActive(false);
                            }
                            else
                                inputText_UI.text += " ";

                            analyzedResult_copy.Clear();
                        }
                        else
                        {
                            isPrevTextWrong = true;
                            inputText_UI.text = prevText + selectedSuggestionWord;
                        }
                    }
                }
            }
            else //send word
            {
                //print(suggestionButtonPressedNum + " pressed " + analyzedResult_copy.Count + "_" + analyzedResult_copy[suggestionButtonPressedNum]);
                if (suggestionButtonPressedNum <= analyzedResult_copy.Count - 1)
                {
                    string selectedSuggestionWord = analyzedResult_copy[suggestionButtonPressedNum];
                    inputText_UI2.text = selectedSuggestionWord;
                    network.sendData("K_" + selectedSuggestionWord);
                }
            }
        }
        else // trackGesture OFF
        {
            trackGesture = false;

            if (strokePointsList.Count != 0) //if something is in the stroke List (if a stroke has been drawn)
            {
                saveList.Clear();
                //print("# of points : " + strokePointsList.Count);
                setState(lineRender, true);
                lineRender.positionCount = strokePointsRenderList.Count;
                for (int i = 0; i < strokePointsRenderList.Count; i++)
                {
                    strokePoint.x = strokePointsRenderList[i].x;
                    strokePoint.y = strokePointsRenderList[i].y;
                    strokePoint.z = strokePointsRenderList[i].z;
                    //strokePoint.z = renderDepth - (renderDepth / 100);
                    lineRender.SetPosition(i, strokePoint);
                }
                Dictionary<string, float> closestFINALList = analysisManager.analyzeInput(strokePointsList, 100);
                saveList = strokePointsList.ToList();

                if (closestFINALList != null)
                {
                    analyzedResult.Clear();
                    analyzedResult_value.Clear();
                    analyzedResult_copy.Clear();

                    var items = from pair in closestFINALList
                                orderby pair.Value descending
                                select pair;
                    int cntItem = 0;
                    foreach (KeyValuePair<string, float> pair in items)
                    {
                        if (cntItem >= 4)
                            break;
                        analyzedResult.Add(pair.Key);
                        analyzedResult_value.Add(pair.Value);
                        cntItem++;
                    }
                    updateSuggestion(analyzedResult);
                    analyzedResult_copy = analyzedResult.ToList();
                    if (datasetCollectionScreenGobj.activeSelf)
                    {
                        if (isPrevTextWrong) // not allowing wrong type in a row
                        {
                            inputText_UI.text = prevText;
                        }
                        prevText = inputText_UI.text;
                        inputText_UI.text += analyzedResult[0];

                        if (curTextList == null)
                        { }
                        else if (analyzedResult[0] == curTextList[textIndex].ToLower()) //if input is correct
                        {
                            isPrevTextWrong = false;
                            textIndex++;
                            // save only when the data's result is above 90, OR the result is above 80, 
                            // and its next suggested word is 10 percentage away from the first
                            if (analyzedResult_value[0] > 90 || (analyzedResult_value[0] > 80 && analyzedResult_value[0] - analyzedResult_value[1] > 10))
                                savePoints(analyzedResult[0], (int)analyzedResult_value[0]); //save to the Dataset

                            if (textIndex >= curTextList.Length) //last word in the sentence
                            {
                                setState(lineRender, false);
                                prevText = "";
                                textIndex = 0;
                                textCurPageNum++;
                                updatePageNum();
                                curTextList = screenManager.getScreenTextByWord(textCurPageNum - 1);
                                text_UI.text = screenManager.getScreenText(textCurPageNum - 1);
                                inputText_UI.text = "";
                                foreach (GameObject gobj in suggestion_UI_Gobj)
                                    gobj.SetActive(false);
                            }
                            else
                            {
                                inputText_UI.text = prevText + curTextList[textIndex - 1]; //to type even the capital lettered words
                                inputText_UI.text += " ";
                            }
                        }
                        else if (analyzedResult[0] != curTextList[textIndex].ToLower())
                            isPrevTextWrong = true;
                    }
                    else //send word
                    {
                        if (analyzedResult_value.Count > 0)
                        {
                            if (analyzedResult_value[0] > 90)
                            {
                                network.sendData("K_" + analyzedResult[0]);
                                inputText_UI2.text = analyzedResult[0];
                            }
                        }
                    }
                }
                strokePointsList.Clear();
                strokePointsRenderList.Clear();
            }
        }

        if (trackGesture)
        {
            Vector3 hitPoint = shootRay();
            //print(hitPoint + "_____" + this.transform.InverseTransformPoint(hitPoint));
            //hitPoint = this.transform.TransformPoint(hitPoint);
            if (hitPoint != Vector3.zero) //if something was hit
            {
                strokePointsRenderList.Add(hitPoint);

                //this.transform.position = hitPoint; //render stroke trail
                hitPoint = this.transform.InverseTransformPoint(hitPoint);
                renderDepth = hitPoint.z;
                strokePointsList.Add(hitPoint);

            }
        }
    }
    private void savePoints(string categoryLabel, int precision)
    {
        dataManager.addData(categoryLabel, saveList);

        //dataManager.removeData(categoryLabel);
        int index = dataManager.getDataCount(categoryLabel) - 1;
        print("[Saved] Word : " + categoryLabel + " (" + precision + "%) | Total # of {" + categoryLabel + "} dataset : " + (index + 1));
        List<Vector2> savedPoints = dataManager.getData(categoryLabel, index);
        renderStrokePoints(savedPoints, userInputRender);
        saveList.Clear();
    }
    IEnumerator optimizer(int optimizerId, float alphaRangeMin, float alphaRangeMax, bool integrateResult = false)
    {
        print("[" + optimizerId + "] Begin optimizing");
        int loopCnt = 0;
        int loopEnd = 1500;
        float sumScore = 0;
        float highestScore = -1;
        float highestScoreAlphaValue = -1;
        Sample sampler = new Sample();
        Analysis analyzer = new Analysis();
        DirectoryInfo[] categoryList = dataManager.getCategoryList();
        float totalFileNum = categoryList.Length * dataManager.getDataCount(categoryList[0].Name);
        float categoryFileNum = categoryList.Length;

        while (loopCnt < loopEnd)
        {
            sumScore = 0;
            float ALPHA_FACTOR = UnityEngine.Random.Range(alphaRangeMin, alphaRangeMax);

            foreach (DirectoryInfo dirInfo in categoryList)// for all the category
            {
                string category = dirInfo.Name;
                WordTemplate template = new WordTemplate(category);//fixed template
                foreach (DirectoryInfo dirInfo_cmp in categoryList)//comparing with all other categories (category == category_tmp : score[10:highest] / category != category_tmp : score[0:highest])
                {
                    string category_cmp = dirInfo_cmp.Name;
                    int numOfDataInOneCategory = dataManager.getDataCount(category_cmp);

                    for (int i = 0; i < numOfDataInOneCategory; i++)
                    {
                        List<Vector2> rawStrokeData = dataManager.getData(category_cmp, i);

                        sampler.invokeSampling(rawStrokeData, 100);
                        List<Vector2> UserStrokeData_normalized = sampler.NormalizedSampledPoints;
                        List<Vector2> UserStrokeDatat_unnormalized = sampler.SampledPoints;
                        List<Vector2> TemplateStrokeData_normalized = template.NormalizedSampledPoints;
                        List<Vector2> TemplateStrokeData_unnormalized = template.SampledPoints;

                        float shapeScore = analyzer.shapeAnalysis(UserStrokeData_normalized, TemplateStrokeData_normalized);
                        float locationScore = analyzer.locationAnalysis(UserStrokeDatat_unnormalized, TemplateStrokeData_unnormalized);


                        float weightedShape = ALPHA_FACTOR * (1.0f - shapeScore / 100);
                        if (weightedShape < 0)
                            weightedShape = ALPHA_FACTOR;
                        float weightedLocation = (1.0f - ALPHA_FACTOR) * (1.0f - locationScore);
                        float finalScore = (weightedShape + weightedLocation) * 100.0f;

                        if (category != category_cmp)
                        {
                            //if (finalScore < 80)
                            //    finalScore = 0;
                            //else
                            finalScore = 100 - finalScore; // 0:highest, 100:lowest
                        }

                        sumScore += finalScore;
                        yield return null;
                    }
                    yield return null;
                }

                yield return null;
            }
            if (highestScore < sumScore)
            {
                highestScore = sumScore;
                highestScoreAlphaValue = ALPHA_FACTOR;
                if (!MID_ALPHA_FACTOR.ContainsKey(optimizerId))
                {
                    MID_ALPHA_FACTOR.Add(optimizerId, highestScoreAlphaValue);
                    MID_HIGHEST_SCORE.Add(optimizerId, highestScore / totalFileNum / categoryFileNum);
                }
                else
                {
                    MID_ALPHA_FACTOR[optimizerId] = highestScoreAlphaValue;
                    MID_HIGHEST_SCORE[optimizerId] = highestScore / totalFileNum / categoryFileNum;
                }


                //MID_ALPHA_FACTOR[optimizerId - 1] = highestScoreAlphaValue;
                //MID_HIGHEST_SCORE[optimizerId - 1] = highestScore / totalFileNum;
            }

            print(optimizerId + ") / [" + (loopCnt + 1) + "/" + loopEnd + "] Alpha : " + ALPHA_FACTOR + " / Score : " + (sumScore / totalFileNum / categoryFileNum) + "__________<Highest : " + highestScoreAlphaValue + " / " + (highestScore / totalFileNum / categoryFileNum) + ">");
            loopCnt++;

            if (integrateResult && loopCnt % 2 == 0)
            {
                var items = from pair in MID_HIGHEST_SCORE
                            orderby pair.Value descending
                            select pair;
                int highestId = -1;
                foreach (KeyValuePair<int, float> pair in items)
                {
                    highestId = pair.Key;
                    break;
                }
                FINAL_ALPHA_FACTOR = MID_ALPHA_FACTOR[highestId];
                FINAL_HIGHEST_SCORE = MID_HIGHEST_SCORE[highestId];

                print("***************** [ ALPHA : " + FINAL_ALPHA_FACTOR + " / SCORE : " + FINAL_HIGHEST_SCORE + " ] *****************");
            }
            yield return null;
            //while (true)
            //{

            //    List<Vector2> pointList = readFromFile(0, userIndex);//raw data, by user #
            //    if (pointList == null)
            //        break;

            //    WordTemplate test_OPEN = new WordTemplate("kitty");

            //    float score = analysisManager.analyzeInput(pointList, 100, false);


            //    float closestScore_TOTAL = -1;
            //    int closestScoreIndex_TOTAL = -1;
            //    float closestScore_shape = 9999999.0f;
            //    int closestScoreIndex_shape = -1;
            //    float closestScore_location = 9999999.0f;
            //    int closestScoreIndex_location = -1;
            //    ////
            //    float closestScore_location2 = 9999999.0f;
            //    int closestScoreIndex_location2 = -1;
            //    ///
            //    Dictionary<string, float> closestShapeList = new Dictionary<string, float>();
            //    Dictionary<string, float> closestLocationList = new Dictionary<string, float>();
            //    Dictionary<string, float> closestLocationList2 = new Dictionary<string, float>();
            //    Dictionary<string, float> closestFINALList = new Dictionary<string, float>();

            //    sampler.invokeSampling(userRawInput, sampleSize);
            //    List<Vector2> normalizedUserInput = sampler.NormalizedSampledPoints;
            //    List<Vector2> unnormalizedUserInput = sampler.SampledPoints;

            //    for (int i = 0; i < templateList.Count; i++)
            //    {
            //        List<Vector2> normalizedtemplate = templateList[i].NormalizedSampledPoints;
            //        List<Vector2> unnormalizedTemplate = templateList[i].SampledPoints;

            //        //////
            //        //List<Vector2> adjustedUserPoints = new List<Vector2>();
            //        //for (int j = 0; j < unnormalizedTemplate.Count; j++) //ADJUSTING LOCATION CHANNEL
            //        //{
            //        //    //adjustedUserPoints.Add(Vector2.Lerp(unnormalizedTemplate[j], unnormalizedUserInput[j], INTERPOLATE_FACTOR)); //big t_val : close to userInput
            //        //    adjustedUserPoints.Add(Vector2.Lerp(unnormalizedTemplate[j], unnormalizedUserInput[j], 0.4f)); //big t_val : close to userInput
            //        //    //adjustedUserPoints.Add(Vector2.Lerp(unnormalizedTemplate[j], unnormalizedUserInput[j], 0.3f)); //big t_val : close to userInput
            //        //}
            //        //////

            //        float shape = analyzer.shapeAnalysis(normalizedUserInput, normalizedtemplate);
            //        //float location = analyzer.locationAnalysis(adjustedUserPoints, unnormalizedTemplate);
            //        float location = analyzer.locationAnalysis(unnormalizedUserInput, unnormalizedTemplate);
            //        //float location2 = analyzer.locationAnalysis(adjustedUserPoints, unnormalizedTemplate);

            //        float weightedShape = ALPHA_FACTOR * (1.0f - shape / sampleSize);
            //        float weightedLocation = (1.0f - ALPHA_FACTOR) * (1.0f - location);
            //        //float weightedLocation2 = (1.0f - ALPHA_FACTOR) * (1.0f - location2);

            //        float score_TOTAL = (weightedShape + weightedLocation) * 100.0f;
            //        //float score_TOTAL2 = (weightedShape + weightedLocation2) * 100.0f;

            //        //do this after finding the optimal alpha value
            //        //alpha value is the 0.76 above
            //        if (closestScore_TOTAL < score_TOTAL)
            //        {
            //            //print((1.0f - shape / sampleSize) + "__" + (1.0f - location / sampleSize) + ":::" + score_TOTAL + ":::" + score_TOTAL2);
            //            closestScore_TOTAL = score_TOTAL;
            //            closestScoreIndex_TOTAL = i;
            //        }
            //        //if (score_TOTAL > 70)
            //        //{
            //        //    closestFINALList.Add(templateList[i].Word, score_TOTAL);
            //        //}
            //    }

            //    string output = "[" + (loopCnt) + "/" + loopEnd + "] " + userIndex + "/ " + (value) + " <" + score + ">";
            //    print(output + " ___________ " + maxScore + "  [" + maxFactorVal + "]");
            //    //file.WriteLine((min / 100.0f) + "," + (max / 100.0f) + "," + score);
            //    scoreSum += score;
            //    userIndex++;
            //    yield return null;
            //}
        }
        yield return null;
    }
    /*
    IEnumerator optimizer2(float alphaRangeMin, float alphaRangeMax)
    {
        print("[2] Begin optimizing");
        int loopCnt = 0;
        int loopEnd = 1500;
        float sumScore = 0;
        float highestScore = -1;
        float highestScoreAlphaValue = -1;
        Sample sampler = new Sample();
        Analysis analyzer = new Analysis();
        DirectoryInfo[] categoryList = dataManager.getCategoryList();
        float totalFileNum = categoryList.Length * dataManager.getDataCount(categoryList[0].Name);

        while (loopCnt < loopEnd)
        {
            sumScore = 0;
            float ALPHA_FACTOR = UnityEngine.Random.Range(alphaRangeMin, alphaRangeMax);

            foreach (DirectoryInfo dirInfo in categoryList)// for all the category
            {
                string category = dirInfo.Name;
                WordTemplate template = new WordTemplate(category);//fixed template
                foreach (DirectoryInfo dirInfo_cmp in categoryList)//comparing with all other categories (category == category_tmp : score[10:highest] / category != category_tmp : score[0:highest])
                {
                    string category_cmp = dirInfo_cmp.Name;
                    int numOfDataInOneCategory = dataManager.getDataCount(category_cmp);

                    for (int i = 0; i < numOfDataInOneCategory; i++)
                    {
                        List<Vector2> rawStrokeData = dataManager.getData(category_cmp, i);

                        sampler.invokeSampling(rawStrokeData, 100);
                        List<Vector2> UserStrokeData_normalized = sampler.NormalizedSampledPoints;
                        List<Vector2> UserStrokeDatat_unnormalized = sampler.SampledPoints;
                        List<Vector2> TemplateStrokeData_normalized = template.NormalizedSampledPoints;
                        List<Vector2> TemplateStrokeData_unnormalized = template.SampledPoints;

                        float shapeScore = analyzer.shapeAnalysis(UserStrokeData_normalized, TemplateStrokeData_normalized);
                        float locationScore = analyzer.locationAnalysis(UserStrokeDatat_unnormalized, TemplateStrokeData_unnormalized);

                        float weightedShape = ALPHA_FACTOR * (1.0f - shapeScore / 100);
                        float weightedLocation = (1.0f - ALPHA_FACTOR) * (1.0f - locationScore);
                        float finalScore = (weightedShape + weightedLocation) * 100.0f;

                        if (category != category_cmp)
                            finalScore = 100 - finalScore; // 0:highest, 100:lowest
                        sumScore += finalScore;
                        yield return null;
                    }
                    yield return null;
                }

                yield return null;
            }
            if (highestScore < sumScore)
            {
                highestScore = sumScore;
                highestScoreAlphaValue = ALPHA_FACTOR;
                MID_ALPHA_FACTOR[1] = highestScoreAlphaValue;
                MID_HIGHEST_SCORE[1] = highestScore / totalFileNum;
            }

            print("[" + (loopCnt + 1) + "/" + loopEnd + "] Alpha : " + ALPHA_FACTOR + " / Score : " + (sumScore / totalFileNum) + "__________<Highest : " + highestScoreAlphaValue + " / " + (highestScore / totalFileNum) + ">");
            loopCnt++;

            yield return null;
        }
        yield return null;
    }
    */
    //IEnumerator optimizer() //interpolation factor optimize
    //{
    //    print("Begin optimizing");
    //    //string path = "Assets/Resources/save_optimizer.csv";

    //    //using (var file = File.CreateText(path))
    //    //{
    //    int loopCnt = 0;
    //    int loopEnd = 1500;
    //    int userIndex = 1;

    //    float maxScore = 0;
    //    float maxFactorVal = 0;
    //    float scoreSum = 0;

    //    while (loopCnt < loopEnd)
    //    {

    //        float value = UnityEngine.Random.Range(0.0f, 100.0f);
    //        AnalysisManager.INTERPOLATE_FACTOR = value / 100.0f;
    //        scoreSum = 0;
    //        userIndex = 1;
    //        while (true)
    //        {
    //            List<Vector2> pointList = readFromFile(0, userIndex);//raw data, by user #
    //            if (pointList == null)
    //                break;

    //            float score = analysisManager.analyzeInput(pointList, 100, false);
    //            string output = "[" + (loopCnt) + "/" + loopEnd + "] " + userIndex + "/ " + (value / 100.0f) + " <" + score + ">";
    //            print(output + " ___________ " + maxScore + "  [" + maxFactorVal + "]");
    //            //file.WriteLine((min / 100.0f) + "," + (max / 100.0f) + "," + score);
    //            scoreSum += score;
    //            userIndex++;
    //            yield return null;
    //        }
    //        if (maxScore < scoreSum)
    //        {
    //            maxScore = scoreSum;
    //            maxFactorVal = AnalysisManager.INTERPOLATE_FACTOR;
    //            print("_________________" + maxScore + "  [" + maxFactorVal + "]_______");
    //        }
    //        loopCnt++;
    //        yield return null;
    //    }

    //    yield return null;
    //    //}
    //}

    //IEnumerator optimizer()  // weight optimize
    //{
    //    print("Begin optimizing");
    //    //string path = "Assets/Resources/save_optimizer.csv";

    //    //using (var file = File.CreateText(path))
    //    //{
    //    int loopCnt = 0;
    //    int loopEnd = 1500;
    //    int userIndex = 1;

    //    float maxScore = 0;
    //    float minIndex = 0;
    //    float maxIndex = 0;

    //    float scoreSum = 0;

    //    while (loopCnt < loopEnd)
    //    {

    //        float min = UnityEngine.Random.Range(0, 100);
    //        float max = UnityEngine.Random.Range(min, 100);
    //        Analysis.MIN = min / 100.0f;
    //        Analysis.MAX = max / 100.0f;
    //        scoreSum = 0;
    //        userIndex = 1;
    //        while (true)
    //        {
    //            List<Vector2> pointList = readFromFile(0, userIndex);//raw data, by user #
    //            if (pointList == null)
    //                break;

    //            float score = analysisManager.analyzeInput(pointList, 100, false);
    //            string output = "[" + (loopCnt) + "/" + loopEnd + "] " + userIndex + "/ " + (min / 100.0f) + "," + (max / 100.0f) + " <" + score + ">";
    //            print(output + " ___________ " + maxScore + "  [" + minIndex + "," + maxIndex + "]");
    //            //file.WriteLine((min / 100.0f) + "," + (max / 100.0f) + "," + score);
    //            scoreSum += score;
    //            userIndex++;
    //            yield return null;
    //        }
    //        if (maxScore < scoreSum)
    //        {
    //            maxScore = scoreSum;
    //            minIndex = min / 100.0f;
    //            maxIndex = max / 100.0f;
    //            print("_________________" + maxScore + "  [" + minIndex + "," + maxIndex + "]_______");
    //        }
    //        //float score = analysisManager.analyzeInput(pointList, 100, false);
    //        //string output = "[" + (loopCnt++) + "/" + loopEnd + "] " + userIndex + "/ " + min + "," + max + " <" + score + ">";
    //        //print(output);
    //        //file.WriteLine(min + "," + max + "," + score);
    //        loopCnt++;
    //        yield return null;
    //    }

    //    yield return null;
    //    //}
    //}
    //public List<Vector2> readFromFile(int type, int userNum, string pathArg = null) //0: raw, 1: normalized, 2: unnormalized
    //{
    //    string[] path = new string[3];
    //    if (pathArg == null)
    //    {
    //        path[0] = "Assets/Resources/raw_input.csv";
    //        path[1] = "Assets/Resources/normalized_input.csv";
    //        path[2] = "Assets/Resources/unnormalized_input.csv";
    //    }
    //    else
    //    {
    //        path[0] = "Assets/Resources/" + pathArg + "_raw_input.csv";
    //        path[1] = "Assets/Resources/" + pathArg + "_normalized_input.csv";
    //        path[2] = "Assets/Resources/" + pathArg + "_unnormalized_input.csv";
    //    }
    //    string configPath = "Assets/Resources/config.csv";
    //    FileInfo fi = new FileInfo(configPath);
    //    if (fi.Exists)
    //    {
    //        using (var file = File.OpenText(configPath))
    //        {
    //            string[] lines = File.ReadAllLines(configPath);
    //            if (lines != null)
    //            {
    //                int index = type > 0 ? 1 : 0;
    //                int maxNum = Int32.Parse((lines[index].Split(','))[1]);
    //                if (userNum > maxNum)
    //                    return null;
    //            }
    //        }
    //    }
    //    List<Vector2> returnList = new List<Vector2>();
    //    using (var file = File.OpenText(path[type]))
    //    {
    //        bool begin = false;
    //        string[] data = File.ReadAllLines(path[type]);
    //        foreach (var line in data)
    //        {
    //            if (line.Contains("USER"))
    //            {
    //                if (begin)
    //                    break;
    //                else if (line == "[USER" + userNum.ToString() + "]")
    //                {
    //                    begin = true;
    //                    continue;
    //                }
    //            }
    //            if (begin)
    //            {
    //                string[] splitData = line.Split(',');
    //                returnList.Add(new Vector2(float.Parse(splitData[0]), float.Parse(splitData[1])));
    //            }
    //        }
    //    }

    //    return returnList;
    //}
    //public void saveToFile(string path, List<Vector2> strokePointsList, bool updateIndex = true)
    //{
    //    string keepText = "# of processed,0";
    //    string userInputNum = "1";
    //    string configPath = "Assets/Resources/config.csv";
    //    FileInfo fi = new FileInfo(configPath);
    //    if (fi.Exists)
    //    {
    //        using (var file = File.OpenText(configPath))
    //        {
    //            string[] lines = File.ReadAllLines(configPath);
    //            if (lines != null)
    //            {
    //                userInputNum = (Int32.Parse((lines[0].Split(','))[1]) + 1).ToString();
    //                keepText = lines[1];
    //            }
    //        }
    //        File.Delete(configPath);
    //    }
    //    using (var file = File.CreateText(configPath))
    //    {
    //        file.WriteLine("# of raw," + userInputNum);
    //        file.WriteLine(keepText);
    //    }

    //    using (var file = File.AppendText(path))
    //    {
    //        file.WriteLine("[USER" + userInputNum + "]");
    //        foreach (var point in strokePointsList)
    //        {
    //            file.WriteLine(point.x + "," + point.y);
    //        }
    //    }
    //}
    //draw spheres at the sampled points
    public void renderStrokePoints(List<Vector2> pointList, GameObject prefab)
    {
        Vector3 tmpVector3 = Vector3.zero;
        foreach (Vector2 v in pointList)
        {
            tmpVector3.x = v.x;
            tmpVector3.y = v.y;
            //tmpVector3.y = v.y + projectionPlane.transform.position.y;
            //tmpVector3.z = renderDepth;// initial value
            tmpVector3.z = renderDepth - 0.05f;
            Destroy(Instantiate(prefab, tmpVector3, Quaternion.identity), 3.0f);

        }
    }
}
