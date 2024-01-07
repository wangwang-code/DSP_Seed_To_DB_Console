using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Security.Principal;
using System.Threading;
using System.Runtime.InteropServices;
using System.Diagnostics;
using static System.Net.Mime.MediaTypeNames;

namespace DSP_Seed_Viewer
{
    class Program
    {
        private static Dictionary<string, string> planetTypeDict = new Dictionary<string, string>
        {
            { "Desert", "沙漠星球数" },
            { "Vocano", "熔岩星球数" },
            { "Ice", "冰冻星球数" },
            { "Ocean", "海洋星球数" },
            { "Gas", "气态星球数" }
        };

        static string getStarTypeString(StarData star)
        {
            string typeString = "";
            if (star.type == EStarType.GiantStar)
                typeString = star.spectr > ESpectrType.K ? (star.spectr > ESpectrType.F ? (star.spectr != ESpectrType.A ? typeString + "蓝巨星" : typeString + "白巨星") : typeString + "黄巨星") : typeString + "红巨星";
            else if (star.type == EStarType.WhiteDwarf)
                typeString += "白矮星";
            else if (star.type == EStarType.NeutronStar)
                typeString += "中子星";
            else if (star.type == EStarType.BlackHole)
                typeString += "黑洞";
            else if (star.type == EStarType.MainSeqStar)
                typeString = typeString + star.spectr + "型恒星";
            return typeString;
        }

        static string getPlanetSingularityString(PlanetData planetData)
        {
            string singularityString = "";
            if (planetData.orbitAround > 0)
                singularityString += "卫星";
            if ((planetData.singularity & EPlanetSingularity.TidalLocked) != EPlanetSingularity.None)
                singularityString += "潮汐锁定永昼永夜";
            if ((planetData.singularity & EPlanetSingularity.TidalLocked2) != EPlanetSingularity.None)
                singularityString += "潮汐锁定1:2";
            if ((planetData.singularity & EPlanetSingularity.TidalLocked4) != EPlanetSingularity.None)
                singularityString += "潮汐锁定1:4";
            if ((planetData.singularity & EPlanetSingularity.LaySide) != EPlanetSingularity.None)
                singularityString += "横躺自转";
            if ((planetData.singularity & EPlanetSingularity.ClockwiseRotate) != EPlanetSingularity.None)
                singularityString += "反向自转";
            if ((planetData.singularity & EPlanetSingularity.MultipleSatellites) != EPlanetSingularity.None)
                singularityString += "多卫星";
            return singularityString;
        }

        static string getPlanetTypeString(PlanetData planetData)
        {
            string typeString = "未知";
            ThemeProto themeProto = LDB.themes.Select(planetData.theme);
            if (themeProto != null)
                typeString = themeProto.DisplayName;
            return typeString;
        }

        static int getMagData(StarData star)
        {
            int result = 0;

            for (int j = 0; j < star.planets.Length; j++)
            {
                var planet = star.planets[j];
                var ResourceCounts = PlanetModelingManager.RefreshPlanetData(planet);

                if (ResourceCounts != null)
                {
                    result += ResourceCounts[14];
                }
                else
                {
                    Console.WriteLine("[Error]Loading resource error");
                    break;
                }
            }
            return result;
        }

        static void seedWork(int currSeed, SQLiteCommand cmd,int starCount, Object lockObject,object lockObject2)
        {
            string tmp = "";
            //Console.WriteLine(currSeed);
            try
            {
                int seedNum = currSeed;
                StarData[] starsc = null;
                GameDesc gd = new GameDesc();
                gd.SetForNewGame(UniverseGen.algoVersion, currSeed, starCount, 1, 1);
                //Console.Write(seedNum + " "+ starCount + " " + UniverseGen.algoVersion + "\n");
                lock (lockObject2)
                {
                    GalaxyData galaxyData = UniverseGen.CreateGalaxy(gd);
                    starsc = (StarData[])galaxyData.stars.Clone();
                }


                if (starsc == null)
                {
                    Console.Write("starsc空");
                    //return null;
                }


                Dictionary<string, int> seedStarsInfo = new Dictionary<string, int>
                    {
                        { "种子号码", seedNum }, // { "最光亮度", 0.0 },
                        { "M型恒星", 0 },{ "K型恒星", 0 },{ "G型恒星", 0 },{ "F型恒星", 0 },{ "A型恒星", 0 },{ "B型恒星", 0 },{ "O型恒星", 0 },
                        { "红巨星", 0 },{ "黄巨星", 0 },{ "白巨星", 0 },{ "蓝巨星", 0 },{ "巨星总数", 0 },
                        { "白矮星", 0 },{ "中子星", 0 },{ "黑洞", 0 },
                        { "最多卫星数", 0 },
                        { "最多潮汐星", 0 },
                        { "潮汐星球数", 0 },
                        { "沙漠星球数", 0 },{ "熔岩星球数", 0 },{ "冰冻星球数", 0 },{ "海洋星球数", 0 },{ "气态星球数", 0 },
                        { "总星球数量", 0 }
                    };

                Dictionary<string, int> seedPlanetsInfo = new Dictionary<string, int>
                    {
                        { "地中海", 0 },{ "水世界", 0 },{ "樱林海", 0 },{ "红石", 0 },{ "海洋丛林", 0 },{ "草原", 0 }, { "热带草原", 0 }, {"潘多拉沼泽", 0 },
                        { "火山灰", 0 },
                        { "猩红冰湖", 0 },{ "熔岩", 0 },
                        { "戈壁", 0 },{ "干旱荒漠", 0 },{ "贫瘠荒漠", 0 },{ "黑石盐滩", 0 },{ "飓风石林", 0 }, { "橙晶荒漠", 0 },
                        { "灰烬冻土", 0 },{ "冰原冻土", 0 }, { "极寒冻土", 0 },
                        { "冰巨星", 0 },{ "气态巨星", 0 }
                    };

                Dictionary<string, int> seedResourcesInfo = new Dictionary<string, int>
                    {
                        { "铁矿脉", 0 },{ "铜矿脉", 0 },{ "硅矿脉", 0 },{ "钛矿脉", 0 },{ "石矿脉", 0 },{ "煤矿脉", 0 },
                        { "原油涌泉", 0 },{ "可燃冰矿", 0 },{ "金伯利矿", 0 },{ "分形硅矿", 0 },{ "有机晶体矿", 0 },{ "光栅石矿", 0 },{ "刺笋矿脉", 0 },
                        { "单极磁矿", 0 }
                    };


                //string outputStr = "";

                float maxLumino = 0.0f;
                float totalLumino = 0.0f;
                //int curMagCount = 0;
                for (int i = 0; i < starsc.Length; i++)
                {
                    //Console.WriteLine(galaxyData.stars.GetType().Name);
                    var star = starsc[i];
                    string currentStarTypeString = getStarTypeString(star);
                    seedStarsInfo[currentStarTypeString] += 1;

                    if (star.dysonLumino > maxLumino) maxLumino = star.dysonLumino;

                    totalLumino += star.dysonLumino;

                    float distanceFromOrigin = (float)(star.uPosition - starsc[0].uPosition).magnitude / 2400000.0f;
                    int starPositionX = (int)Math.Round(star.uPosition.x, 0, MidpointRounding.AwayFromZero);
                    int starPositionY = (int)Math.Round(star.uPosition.y, 0, MidpointRounding.AwayFromZero);
                    int starPositionZ = (int)Math.Round(star.uPosition.z, 0, MidpointRounding.AwayFromZero);

                    bool isInDsp = star.dysonRadius * 2 > star.planets[0].sunDistance;
                    string isInDspText = isInDsp ? "是" : "否";

                    /*
                    int cxsdcount = 0;
                    int[] resource = new int[LDB.veins.Length];
                    var planetCount1 = 0;
                    float gasSpeed = 0;
                    string planetNameInfo = "";
                    */

                    string planetsType = "";
                    string planetsString = "";
                    int moons = 0;
                    int maxMoons = 0;
                    int tidals = 0;
                    bool hasWater = false;
                    bool hasAcid = false;

                    List<int> resourceCountList = new List<int>() { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };

                    for (int j = 0; j < star.planets.Length; j++)
                    {
                        var planet = star.planets[j];
                        string currentPlanetString = getPlanetTypeString(planet);

                        seedPlanetsInfo[currentPlanetString] += 1;

                        if (planet.waterItemId == 1000)
                            hasWater = true;
                        if (planet.waterItemId == 1116)
                            hasAcid = true;

                        string singularityString = "";
                        if (planet.orbitAround > 0)
                            singularityString += "卫星";
                        if ((planet.singularity & EPlanetSingularity.TidalLocked) != EPlanetSingularity.None)
                            singularityString += "潮汐锁定永昼永夜";
                        if ((planet.singularity & EPlanetSingularity.MultipleSatellites) != EPlanetSingularity.None)
                            singularityString += "多卫星";

                        if (singularityString.Contains("潮汐锁定永昼永夜"))
                        {
                            seedStarsInfo["潮汐星球数"] += 1;
                            planetsString += "=";
                            planetsType += "=";
                            tidals += 1;
                        }
                        planetsString += currentPlanetString;
                        planetsType += planet.type;
                        if (planet.type.ToString() == "Gas") moons = 0;

                        if (singularityString.Contains("多卫星"))
                        {
                            planetsString += "@@";
                            planetsType += "@@";
                        }
                        else if (singularityString.Contains("卫星"))
                        {
                            planetsString += "@";
                            planetsType += "@";
                            moons += 1;
                            if (moons > maxMoons) maxMoons = moons;
                        }

                        if (j < star.planets.Length - 1) planetsString += "|";
                        if (j < star.planets.Length - 1) planetsType += "|";

                        seedStarsInfo[planetTypeDict[planet.type.ToString()]] += 1;

                        var counts = PlanetModelingManager.RefreshPlanetData(planet);
                        if (counts != null)
                        {
                            for (int k = 0; k < LDB.veins.Length; k++)
                            {
                                resourceCountList[k] += counts[k + 1];
                            }
                        }

                        /*
                        if (!string.IsNullOrEmpty(planet.singularityString))
                            planetNameInfo += "-" + planet.singularityString;
                        planetNameInfo += ";";
                        if (planet.typeString == "气态巨星")
                        {
                            if (gasSpeed < planet.gasSpeeds[1])
                                gasSpeed = planet.gasSpeeds[1];
                        }
                        if (planet.orbitAroundPlanet != null)
                        {
                            planetCount1++;
                        }
                        if (planet.singularityString.Contains("潮汐锁定"))
                        {
                            cxsdcount++;
                        }
                        */

                    }

                    //using (SQLiteTransaction tr = cn.BeginTransaction())
                    //{

                    //cmd.CommandText += "INSERT INTO seedGalaxiesInfo(seedNum) VALUES (1233); ";
                    
                    tmp += "INSERT INTO seedGalaxiesInfo(" +
                                      "seedNum, 星系名称, 星系类型, 恒星光度, 环盖首星, " +
                                      "星系距离, 星系坐标X, 星系坐标Y, 星系坐标Z, " +
                                      "潮汐星数, 最多卫星, 星球数量, 星球类型, 星球类名, 是否有水, 有硫酸否, " +
                                      "铁矿脉, 铜矿脉, 硅矿脉, 钛矿脉, 石矿脉, 煤矿脉, 原油涌泉, 可燃冰矿, " +
                                      "金伯利矿, 分形硅矿, 有机晶体矿, 光栅石矿, 刺笋矿脉, 单极磁矿" +
                                      ") VALUES (" +
                    "'" + seedNum.ToString() + "', " +
                    "'" + star.name.Replace("'", "''") + "', " +
                    "'" + currentStarTypeString + "', " +
                    "'" + star.dysonLumino.ToString("F4") + "', " +
                    "'" + isInDspText + "', " +
                    "'" + distanceFromOrigin.ToString("F3") + "', " +
                    "'" + starPositionX.ToString() + "', " +
                    "'" + starPositionY.ToString() + "', " +
                    "'" + starPositionZ.ToString() + "', " +
                    "'" + tidals.ToString() + "', " +
                    "'" + maxMoons.ToString() + "', " +
                    "'" + star.planetCount.ToString() + "', " +
                    "'" + planetsType + "', " +
                    "'" + planetsString + "', " +
                    "'" + (hasWater ? "是" : "否") + "', " +
                    "'" + (hasAcid ? "是" : "否") + "', " +
                    "'" + resourceCountList[0].ToString() + "', " +
                    "'" + resourceCountList[1].ToString() + "', " +
                    "'" + resourceCountList[2].ToString() + "', " +
                    "'" + resourceCountList[3].ToString() + "', " +
                    "'" + resourceCountList[4].ToString() + "', " +
                    "'" + resourceCountList[5].ToString() + "', " +
                    "'" + resourceCountList[6].ToString() + "', " +
                    "'" + resourceCountList[7].ToString() + "', " +
                    "'" + resourceCountList[8].ToString() + "', " +
                    "'" + resourceCountList[9].ToString() + "', " +
                    "'" + resourceCountList[10].ToString() + "', " +
                    "'" + resourceCountList[11].ToString() + "', " +
                    "'" + resourceCountList[12].ToString() + "', " +
                    "'" + resourceCountList[13].ToString() + "' " +
                    "); ";

                    //cmd.Parameters.Add("seedNum", DbType.Int32).Value = seedNum;
                    //cmd.Parameters.Add("starName", DbType.String).Value = star.name;
                    //cmd.Parameters.Add("typeString", DbType.String).Value = star.typeString;
                    //cmd.Parameters.Add("dysonLumino", DbType.Double).Value = Convert.ToDouble(star.dysonLumino.ToString("F4"));
                    //cmd.Parameters.Add("coordinateFromOrigin", DbType.String).Value = distanceFromOrigin.ToString("F3");
                    //cmd.Parameters.Add("tidalPlanets", DbType.Int32).Value = tidals;
                    //cmd.Parameters.Add("maxMoons", DbType.Int32).Value = maxMoons;
                    //cmd.Parameters.Add("planetCount", DbType.Int32).Value = star.planetCount;
                    //cmd.Parameters.Add("planetsType", DbType.String).Value = planetsType;
                    //cmd.Parameters.Add("planetsString", DbType.String).Value = planetsString;

                    //cmd.ExecuteNonQuery();
                    //tr.Commit();
                    //}

                    if (tidals > seedStarsInfo["最多潮汐星"]) seedStarsInfo["最多潮汐星"] = tidals;
                    if (maxMoons > seedStarsInfo["最多卫星数"]) seedStarsInfo["最多卫星数"] = maxMoons;

                    seedStarsInfo["总星球数量"] += star.planetCount;

                    /*
                     * if (star.type == EStarType.BlackHole || star.type == EStarType.NeutronStar)
                    {
                        curMagCount += getMagData(star);
                    }
                    */

                    for (int k = 0; k < resourceCountList.Count; k++)
                    {
                        switch (k)
                        {
                            case 0: seedResourcesInfo["铁矿脉"] += resourceCountList[k]; break;
                            case 1: seedResourcesInfo["铜矿脉"] += resourceCountList[k]; break;
                            case 2: seedResourcesInfo["硅矿脉"] += resourceCountList[k]; break;
                            case 3: seedResourcesInfo["钛矿脉"] += resourceCountList[k]; break;
                            case 4: seedResourcesInfo["石矿脉"] += resourceCountList[k]; break;
                            case 5: seedResourcesInfo["煤矿脉"] += resourceCountList[k]; break;
                            case 6: seedResourcesInfo["原油涌泉"] += resourceCountList[k]; break;
                            case 7: seedResourcesInfo["可燃冰矿"] += resourceCountList[k]; break;
                            case 8: seedResourcesInfo["金伯利矿"] += resourceCountList[k]; break;
                            case 9: seedResourcesInfo["分形硅矿"] += resourceCountList[k]; break;
                            case 10: seedResourcesInfo["有机晶体矿"] += resourceCountList[k]; break;
                            case 11: seedResourcesInfo["光栅石矿"] += resourceCountList[k]; break;
                            case 12: seedResourcesInfo["刺笋矿脉"] += resourceCountList[k]; break;
                            case 13: seedResourcesInfo["单极磁矿"] += resourceCountList[k]; break;
                            default: break;
                        }
                    }

                    //singleTitle += planetNameInfo + "," + gasSpeed + "," + planetCount1 + "," + cxsdcount +
                    //                "," + (hasWater ? "是," : "否,") + (hasAcid ? "是," : "否,");

                    //singleTitle += planetNameInfo;

                    //singleTitle += "\n";

                    //Console.Write(outputStr);
                    //Console.Write("\n");
                }

                seedStarsInfo["巨星总数"] = seedStarsInfo["红巨星"] + seedStarsInfo["黄巨星"] + seedStarsInfo["白巨星"] + seedStarsInfo["蓝巨星"];

                //using (SQLiteTransaction tr = cn.BeginTransaction())
                //{

                tmp += "INSERT INTO seedStarsInfo (seedNum, " +
                                  "M型恒星, K型恒星, G型恒星, F型恒星, A型恒星, B型恒星, O型恒星, " +
                                  "红巨星, 黄巨星, 白巨星, 蓝巨星, 白矮星, 中子星, 黑洞, " +
                                  "巨星数, 最多卫星, 最多潮汐星, 潮汐星球数, " +
                                  "沙漠星球数, 熔岩星球数, 冰冻星球数, 海洋星球数, 气态星球数, 总星球数量, " +
                                  "最光亮度, 星球总亮度" +
                                  ") VALUES (" +
                                  seedStarsInfo["种子号码"].ToString() + ", " +
                                  seedStarsInfo["M型恒星"].ToString() + ", " +
                                  seedStarsInfo["K型恒星"].ToString() + ", " +
                                  seedStarsInfo["G型恒星"].ToString() + ", " +
                                  seedStarsInfo["F型恒星"].ToString() + ", " +
                                  seedStarsInfo["A型恒星"].ToString() + ", " +
                                  seedStarsInfo["B型恒星"].ToString() + ", " +
                                  seedStarsInfo["O型恒星"].ToString() + ", " +
                                  seedStarsInfo["红巨星"].ToString() + ", " +
                                  seedStarsInfo["黄巨星"].ToString() + ", " +
                                  seedStarsInfo["白巨星"].ToString() + ", " +
                                  seedStarsInfo["蓝巨星"].ToString() + ", " +
                                  seedStarsInfo["白矮星"].ToString() + ", " +
                                  seedStarsInfo["中子星"].ToString() + ", " +
                                  seedStarsInfo["黑洞"].ToString() + ", " +
                                  seedStarsInfo["巨星总数"].ToString() + ", " +
                                  seedStarsInfo["最多卫星数"].ToString() + ", " +
                                  seedStarsInfo["最多潮汐星"].ToString() + ", " +
                                  seedStarsInfo["潮汐星球数"].ToString() + ", " +
                                  seedStarsInfo["沙漠星球数"].ToString() + ", " +
                                  seedStarsInfo["熔岩星球数"].ToString() + ", " +
                                  seedStarsInfo["冰冻星球数"].ToString() + ", " +
                                  seedStarsInfo["海洋星球数"].ToString() + ", " +
                                  seedStarsInfo["气态星球数"].ToString() + ", " +
                                  seedStarsInfo["总星球数量"].ToString() + ", " +
                                  maxLumino.ToString("F4") + ", " +
                                  totalLumino.ToString("F4") + "); ";

                tmp += "INSERT INTO seedPlanetsStringInfo (seedNum, " +
                                    "地中海, 水世界, 樱林海, 红石, 海洋丛林, 草原, 热带草原, 潘多拉沼泽, " +
                                    "火山灰, 猩红冰湖, 熔岩, " +
                                    "戈壁, 干旱荒漠, 贫瘠荒漠, 黑石盐滩, 飓风石林, 橙晶荒漠, " +
                                    "灰烬冻土, 冰原冻土, 极寒冻土, 冰巨星, 气态巨星" +
                                    ") VALUES (" +
                                    seedStarsInfo["种子号码"].ToString() + ", " +
                                    seedPlanetsInfo["地中海"].ToString() + ", " +
                                    seedPlanetsInfo["水世界"].ToString() + ", " +
                                    seedPlanetsInfo["樱林海"].ToString() + ", " +
                                    seedPlanetsInfo["红石"].ToString() + ", " +
                                    seedPlanetsInfo["海洋丛林"].ToString() + ", " +
                                    seedPlanetsInfo["草原"].ToString() + ", " +
                                    seedPlanetsInfo["热带草原"].ToString() + ", " +
                                    seedPlanetsInfo["潘多拉沼泽"].ToString() + ", " +
                                    seedPlanetsInfo["火山灰"].ToString() + ", " +
                                    seedPlanetsInfo["猩红冰湖"].ToString() + ", " +
                                    seedPlanetsInfo["熔岩"].ToString() + ", " +
                                    seedPlanetsInfo["戈壁"].ToString() + ", " +
                                    seedPlanetsInfo["干旱荒漠"].ToString() + ", " +
                                    seedPlanetsInfo["贫瘠荒漠"].ToString() + ", " +
                                    seedPlanetsInfo["黑石盐滩"].ToString() + ", " +
                                    seedPlanetsInfo["飓风石林"].ToString() + ", " +
                                    seedPlanetsInfo["橙晶荒漠"].ToString() + ", " +
                                    seedPlanetsInfo["灰烬冻土"].ToString() + ", " +
                                    seedPlanetsInfo["冰原冻土"].ToString() + ", " +
                                    seedPlanetsInfo["极寒冻土"].ToString() + ", " +
                                    seedPlanetsInfo["冰巨星"].ToString() + ", " +
                                    seedPlanetsInfo["气态巨星"].ToString() + "); ";

                tmp += "INSERT INTO seedResourcesInfo (seedNum, " +
                                    "铁矿脉, 铜矿脉, 硅矿脉, 钛矿脉, 石矿脉, 煤矿脉, 原油涌泉, 可燃冰矿, " +
                                    "金伯利矿, 分形硅矿, 有机晶体矿, 光栅石矿, 刺笋矿脉, 单极磁矿" +
                                    ") VALUES (" +
                                    seedStarsInfo["种子号码"].ToString() + ", " +
                                    seedResourcesInfo["铁矿脉"].ToString() + ", " +
                                    seedResourcesInfo["铜矿脉"].ToString() + ", " +
                                    seedResourcesInfo["硅矿脉"].ToString() + ", " +
                                    seedResourcesInfo["钛矿脉"].ToString() + ", " +
                                    seedResourcesInfo["石矿脉"].ToString() + ", " +
                                    seedResourcesInfo["煤矿脉"].ToString() + ", " +
                                    seedResourcesInfo["原油涌泉"].ToString() + ", " +
                                    seedResourcesInfo["可燃冰矿"].ToString() + ", " +
                                    seedResourcesInfo["金伯利矿"].ToString() + ", " +
                                    seedResourcesInfo["分形硅矿"].ToString() + ", " +
                                    seedResourcesInfo["有机晶体矿"].ToString() + ", " +
                                    seedResourcesInfo["光栅石矿"].ToString() + ", " +
                                    seedResourcesInfo["刺笋矿脉"].ToString() + ", " +
                                    seedResourcesInfo["单极磁矿"].ToString() + "); ";

                //Console.WriteLine(cmd.CommandText);

                //cmd.ExecuteNonQuery();
                //cmd.CommandText = "";
                //}
                lock (lockObject)
                {
                    cmd.CommandText = tmp;
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = "";
                }
            }
            catch (Exception e)
            {
                //tr.Rollback();
                Console.WriteLine("Error occurred at " + currSeed);
                Console.WriteLine(e);
            }
            //return null;
        }

        static void cmdWork(SQLiteCommand cmd,string cmdBuilders)
        {
            cmd.CommandText = cmdBuilders;
            cmd.ExecuteNonQuery();
            cmd.CommandText = "";
        }
        static void Main(string[] args)
        {
            Stopwatch stopwatch = new Stopwatch();
            object lockObject1 = new object();
            object lockObject2 = new object();
            PlanetModelingManager.Start();
            StringBuilder cmdBuilder = new StringBuilder();
            int startSeed = 0;
            int endSeed = 100000000;
            int starCount = 64;

            Console.WriteLine("请输入起始种子号:");
            startSeed = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("请输入结束种子号");
            endSeed = Convert.ToInt32(Console.ReadLine());
            Console.WriteLine("请输入星系数");
            starCount = Convert.ToInt32(Console.ReadLine());

            stopwatch.Start();

            const int threadsPerBatch = 1000;
            Thread[] threads = new Thread[threadsPerBatch];

            Console.WriteLine("Begining to generate seed to DB");
            string currDir = System.IO.Directory.GetCurrentDirectory();
            string dbName = currDir + "\\DSP_Seeds.db";
            SQLiteConnection cn = new SQLiteConnection("data source=" + dbName);
            cn.Open();
            SQLiteCommand cmd = new SQLiteCommand();
            cmd.Connection = cn;

            SQLiteTransaction tr = cn.BeginTransaction();
            cmd.CommandText = "";
            Thread cmdthread = new Thread(() => { });
            int a = 0;
            for (int currSeed = startSeed; currSeed < endSeed; currSeed++)
            {
                //WorkerParams workerParams = new WorkerParams(currSeed, cmdBuilder, starCount);


                //Console.WriteLine(currSeed);
                int currSeedc = currSeed;
                threads[a % threadsPerBatch] = new Thread(() => {

                    seedWork(currSeedc, cmd, starCount, lockObject1, lockObject1); 
                }
                );
                threads[a % threadsPerBatch].Start();
                
                    //Console.Write("ExecuteNonQuery");
                    /**
                    foreach (Thread thread in threads)
                    {
                        if (thread != null)
                        {
                            //Console.WriteLine("slpee " + a);
                            thread.Join();

                        }
                    }*/
                    //string cmdBuilders = (string)cmdBuilder.ToString().Clone();
                    /**
                    if (cmdthread.IsAlive) cmdthread.Join();
                    if (currSeed == endSeed - 1 && cmdthread == null)
                    {
                        string cmdBuilders = (string)cmdBuilder.ToString().Clone();
                        cmd.CommandText = cmdBuilders;
                        cmd.ExecuteNonQuery();
                        cmd.CommandText = "";
                        cmdBuilder = new StringBuilder();
                    }if (a % 10 == 0)
                    {
                        Console.WriteLine("cmdWork " + a);
                        string cmdBuilders = (string)cmdBuilder.ToString().Clone();
                        cmdthread = new Thread(() =>
                        {
                            cmdWork(cmd, cmdBuilders);
                        });
                        cmdthread.Start();
                        cmdBuilder = new StringBuilder();
                    }**/
                if (a % 20 == 0)
                {
                    lock (lockObject1)
                    {
                        tr.Commit();
                        tr = cn.BeginTransaction();
                        //Console.WriteLine(currSeed + "Commit");
                        cmd = new SQLiteCommand();
                        cmd.Connection = cn;
                        cmd.CommandText = "";
                    }

                }/*
                if (currSeed % 10000 == 9999)
                {
                    tr.Commit();
                    tr = cn.BeginTransaction();
                    Console.WriteLine(currSeed + "Commit");
                }*/
                a++;

                //Console.WriteLine(currSeed);
            }
            foreach (Thread thread in threads)
            {
                if (thread != null)
                {
                    //Console.WriteLine("slpee " + a);
                    thread.Join();

                }
            }/*
            lock (lockObject)
            {
                string cmdBuilderc = cmdBuilder.ToString();
                cmd.CommandText = cmdBuilderc;
            }
            cmd.ExecuteNonQuery();
            */
            tr.Commit();
            tr = cn.BeginTransaction();

            //cn.Close();
            Console.Write("done");
            stopwatch.Stop();
            Console.WriteLine("耗时: " + stopwatch.Elapsed);
            Console.ReadKey();
        }
    }
}