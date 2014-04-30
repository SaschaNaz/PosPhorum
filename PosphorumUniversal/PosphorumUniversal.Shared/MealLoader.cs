using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Windows.Web.Http;

namespace PosphorumUniversal
{
    class MealLoader
    {
        public async Task<XDocument> GetDietmenu()
        {
            /*
             * 자 전체 구조는 이런거죠
             * Dietmenu 안에
             * Day가 들어 있고
             * Day 안에
             * Time이 들어 있고 - 이때 속성으로 아침점심저녁
             * Time 안에 각 Foods가 들어 있죠 그런거죠! - 이때 속성으로 ABCDEFG (?
             * 
             */

            XDocument xdietmenu = new XDocument();
            XElement dietofweek = new XElement("DietofWeek");
            using (HttpClient client = new HttpClient())
            {
                using (HttpResponseMessage response = await client.GetAsync(new Uri("http://page.postech.ac.kr/res1/")))
                {
                    String str = await response.Content.ReadAsStringAsync();
                    await Task.Run(() =>
                    {
                        str = str.Replace("&nbsp;", "\u0020")
                            .Replace("&shy;", "\u00AD");
                        //str = str.Remove(str.IndexOf("<![if !supportMisalignedRows]>"), str.IndexOf("<![endif]>") - str.IndexOf("<![if !supportMisalignedRows]>") + 10);

                        str = Regex.Replace(str, @"<(\w+)", m => m.Value.ToLower());
                        str = Regex.Replace(str, @"<br>", "<br/>");
                        str = Regex.Replace(str, @"(</tbody>)\s+(</table>)\s+</div>", "$1$2</body>");

                        //<col>s
                        str = Regex.Replace(str, @"(<col\s.+>)", "$1</col>");

                        //Illegal attributes
                        str = Regex.Replace(str, @"\w+=#?\w+", "");

                        //I don't know why but there are some tags like <TD style="..." 8  >
                        str = Regex.Replace(str, @"\s[0-9]\s+\/?>", ">");
                    });
                    XElement xelm = XElement.Parse(str);
                    XElement[] tablerows = null;
                    {
                        XElement body = xelm.Element("body");
                        XElement[] tables = body.Descendants("tbody").ToArray();
                        foreach (XElement table in tables)
                        {
                            XElement[] rows = table.Elements().ToArray();
                            if (rows.Length >= 14)
                            {
                                tablerows = rows;
                                break;
                            }
                        }
                    }

                    var startingRow = 3;
                    for (Int32 i = startingRow; i <= startingRow + 14; i += 2)//each two rows are for a day
                    {
                        XElement xday = new XElement("Day");
                        XElement[] columns = tablerows[i].Elements().ToArray();
                        XElement[] columnscal = tablerows[i + 1].Elements().ToArray();

                        #region 날짜
                        {
                            String[] daydayofweek = columns[0]
                                .Elements().First()//p
                                .Elements().First()//font
                                .Value.Split('(');//날짜 및 요일. Filtering by Split('\n') seldomly fails because '\n' disappears randomly.
                            String[] monthday = SplitIntoTwoBySlash(daydayofweek[0]);
                            xday.Add(
                                new XAttribute("Month", Convert.ToInt32(monthday[0])),
                                new XAttribute("Day", Convert.ToInt32(monthday[1])));
                            //new XAttribute("DayofWeek", daydayofweek[1][1])); DateTime 클래스는 이미 DayOfWeek 프로퍼티를 갖고 있다. 따라서 또 넣을 필요는 없음.
                        }
                        #endregion

                        //아침
                        xday.Add(
                            new XElement("Time",
                                new XAttribute("When", "Breakfast"),
                                MakeFoodsElement(columns[1], columnscal[1], "A"),
                                MakeFoodsElement(columns[2], columnscal[2], "B")));

                        //점심
                        xday.Add(
                            new XElement("Time",
                                new XAttribute("When", "Lunch"),
                                MakeFoodsElement(columns[3], columnscal[3], "A"),
                                MakeDualFoodsElement(columns[5], columnscal[5], "C", "D")));

                        //저녁
                        xday.Add(
                            new XElement("Time",
                                new XAttribute("When", "Dinner"),
                                MakeFoodsElement(columns[4], columnscal[4], "A"),
                                columns.Length > 6 ? MakeDualFoodsElement(columns[6], columnscal[6], "C", "D") : null));

                        dietofweek.Add(xday);
                    }
                    xdietmenu.Add(dietofweek);
                }
            }
            //지금은 시간이랑 타입을 하드코딩해서 넣지만 맨 윗줄에서 읽어들여서 자동화하는 방법을 생각해봅시다

            return xdietmenu;
        }

        //그냥 두 줄씩 떼어서 첫줄은 한국어 둘째줄은 영어 하면 될 것 같죠? ㅎㅎ... 쓰다가 영어 한 줄 빼먹은 걸 보셔야...
        //그러므로, 첫줄 한국어면 다음 영어 찾고, 없고 다음줄이 또 한국어면 그냥 넘기고 다음 Food 작성
        //한국어가 안 들어오고 영어가 먼저 들어왔다면 그냥 넘기고 다음 Food 작성
        /// <summary>
        /// 파싱하고 있는 테이블 중 한 칸 떼어 넘기면 파싱된 결과물을 줍니다!
        /// </summary>
        /// <param name="column">파싱하고 있는 테이블 중 한 칸</param>
        /// <param name="type">어느 코너에 나오는 음식인가요</param>
        /// <returns></returns>
        XElement MakeFoodsElement(XElement column, XElement column2, String type)
        {
            List<String> strfood = new List<String>();
            {
                XElement[] foodcode = column
                    .Elements().ToArray();//p
                foreach (XElement code in foodcode)
                {
                    String str = "";
                    foreach (XElement fontcode in code.Elements())//p 안에 태그가 한 개만 있을 거 같죠? ㅎㅎㅎ... 내용 빈 태그 한번 보셔야...
                    {
                        str += fontcode.Value;
                    }
                    foreach (String line in str.Split('\n'))//줄마다 p로 나뉘어 있는 경우가 있고 p 안에 두 줄이 있는 경우가 있다, 후자 대응
                    {
                        var trimmed = line.Trim();
                        if (trimmed.Length > 0)
                            strfood.Add(trimmed);
                    }
                }
            }

            if (strfood.Count > 1)
            {
                String calint = "";
                {
                    XElement[] foodcode = column2
                        .Elements().ToArray();//p
                    foreach (XElement code in foodcode)
                    {
                        foreach (XElement fontcode in code.Elements())//font 태그는 한 개만 있을 거 같죠? ㅎㅎㅎ... 내용 빈 font 태그 한번 보셔야...
                        {
                            calint += fontcode.Value;
                        }
                    }
                }
                return ParseSingleFoodData(strfood, calint, type);
            }
            else
                return null;
        }

        /// <summary>
        /// 기존 포스로이드 앺과의 호환성 때문에 C/D는 합쳐놓은 듯한데... 그런 거 없고 따로 표시합니다. 그러려고 만들었어요. 역시 파싱하고 있는 테이블 중 한 칸 떼어 넘기면 파싱된 결과물을 줍니다.
        /// </summary>
        /// <param name="column">파싱하고 있는 테이블 중 한 칸</param>
        /// <param name="type1">어느 코너에 나오는 음식인가요1</param>
        /// <param name="type2">어느 코너에 나오는 음식인가요2</param>
        /// <returns></returns>
        XElement[] MakeDualFoodsElement(XElement column, XElement column2, String type1, String type2)
        {
            List<String> strfood = new List<String>();
            {
                XElement[] foodcode = column
                    .Elements().ToArray();//p
                foreach (XElement code in foodcode)
                {
                    String str = "";
                    foreach (XElement fontcode in code.Elements())//font 태그는 한 개만 있을 거 같죠? ㅎㅎㅎ... 내용 빈 font 태그 한번 보셔야...
                    {
                        str += fontcode.Value;
                    }
                    if (str.Length > 0)//빈 문자열은 갖다 버린다!.. 왜 엔터는 쳐서 빈 곳을 만들어요 으아아
                    {
                        if (strfood.Count > 0)
                        {
                            String laststr = strfood.Last();
                            Char last = laststr[laststr.Length - 1];
                            Char first = str[0];
                            if ((last == '/' && first != '/') || (last != '/' && first == '/'))
                            {
                                strfood.RemoveAt(strfood.Count - 1);
                                str = laststr + str;
                            }
                        }
                        strfood.Add(str);
                    }
                }
            }

            Boolean IsThereNoType1 = false, IsThereNoType2 = false;//C, D는 항상 함께 있을 거 같죠? ㅎㅎㅎㅎㅎㅎㅎ.... 'D코너' 한줄 추가되어 있고 C코너 사라진 꼴을 보셔야 ㅎㅎㅎ

            if (strfood.Count > 1)//코너 하나만 있는지 모두 있는지 판별. 하나만 있는 경우는 최상단에 해당 코너 이름을 써 주는 관습이 있습니다.
            {
                if (strfood.First().Contains(String.Format("{0}코너", type1)))//왜 StartsWith 안쓰고 Contains 쓰냐면... 어떤주엔 'D코너' 써놓고 어떤주엔 '<D코너>' 써놓거든요. 제발 <D>는 안 썼으면...
                {
                    IsThereNoType2 = true;
                    strfood.Remove(strfood.First());
                }
                else if (strfood.First().Contains(String.Format("{0}코너", type2)))
                {
                    IsThereNoType1 = true;
                    strfood.Remove(strfood.First());
                }//첫번째 줄을 무조건 지우면 안 돼요... 홀수인 이유가 코너이름 적으려고도 있겠지만 엔터치고 괄호안에 원산지 등 추가정보 적으면서 저렇게 되는 경우가 있어서...
            }

            if (strfood.Count > 1)//<행사이름> 써 놓고 그 이상 아무것도 없을 때 오류 방지
            {
                String calint = "";
                {
                    XElement[] foodcode = column2
                        .Elements().ToArray();//p
                    foreach (XElement code in foodcode)
                    {
                        foreach (XElement fontcode in code.Elements())//font 태그는 한 개만 있을 거 같죠? ㅎㅎㅎ... 내용 빈 font 태그 한번 보셔야...
                        {
                            calint += fontcode.Value;
                        }
                    }
                }

                if (!IsThereNoType1 && !IsThereNoType2)
                {
                    for (Int32 i = 1; i < strfood.Count; i++)//기본적으론 C와 D는 슬래시로만 구분되지만, 슬래시 앞에 엔터로 또 구분되어 있는 경우가 있다
                    {
                        if (strfood[i][0] == '/')
                        {
                            strfood[i - 1] += strfood[i];
                            strfood.Remove(strfood[i]);
                            i--;
                        }
                    }

                    XElement xfoods1 = new XElement("Foods", new XAttribute("Type", type1));
                    XElement xfoods2 = new XElement("Foods", new XAttribute("Type", type2));
                    if (calint.Length > 0)
                    {
                        String[] splittedcalories = SplitIntoTwoBySlash(calint);
                        if (splittedcalories.Length > 1)
                        {
                            xfoods1.Add(new XAttribute("Calories", splittedcalories[0]));
                            xfoods2.Add(new XAttribute("Calories", splittedcalories[1]));
                        }
                        else //이건 중간에 슬래시를 빼 먹은 것
                        {
                            Int32 length = splittedcalories[0].Length;
                            if (length > 4)
                            {
                                if (length % 2 == 1 && splittedcalories[0][0] < splittedcalories[0][length / 2])//길이가 같거나, 앞 식단 자리수가 뒷 식단 자리수보다 큼
                                {
                                    xfoods1.Add(new XAttribute("Calories", splittedcalories[0].Substring(0, length / 2 + 1)));
                                    xfoods2.Add(new XAttribute("Calories", splittedcalories[0].Substring(length / 2 + 1)));
                                }
                                else
                                {
                                    xfoods1.Add(new XAttribute("Calories", splittedcalories[0].Substring(0, length / 2)));
                                    xfoods2.Add(new XAttribute("Calories", splittedcalories[0].Substring(length / 2)));
                                }
                            }
                            else
                            {
                                xfoods1.Add(new XAttribute("Calories", splittedcalories[0]));
                                xfoods2.Add(new XAttribute("Calories", -1));
                            }
                        }
                    }
                    else
                    {
                        xfoods1.Add(new XAttribute("Calories", -1));
                        xfoods2.Add(new XAttribute("Calories", -1));
                    }

                    Char FormerLineLanguage = 'E';//K for Korean, E for English
                    XElement xfood1 = null;
                    XElement xfood2 = null;
                    foreach (String str in strfood)
                    {
                        Nullable<BracketData> bdata = ExtractBracket(str);//가끔 괄호 안에 '/'가 있는 바람에 파싱을 방해하므로 빼 놨다가 나중에 다시 붙임
                        String processed = String.Empty;
                        if (bdata.HasValue)
                        {
                            processed = bdata.Value.AfterExtracted;
                            if (processed.Length == 0)
                            {
                                //마지막 코드에 괄호 추가, 더 할 일 없음. 바로 브레이크
                                XElement langToBeFixed = xfood2.Elements().Last();
                                XAttribute fixstr = langToBeFixed.Attribute("Value");
                                fixstr.Remove();
                                langToBeFixed.Add(new XAttribute("Value", (String)fixstr + bdata.Value.ExtractedString));
                                break;
                            }
                        }

                        //문자열 처리부분
                        String[] splitted = SplitIntoTwoBySlashWithException(str);
                        Char PresentLineLanguage;
                        if (IsEnglish(str))
                            PresentLineLanguage = 'E';
                        else
                            PresentLineLanguage = 'K';
                        switch (FormerLineLanguage)
                        {
                            case 'E':
                                switch (PresentLineLanguage)
                                {
                                    case 'E':
                                        {
                                            xfoods1.Add(xfood1);
                                            xfoods2.Add(xfood2);
                                            xfood1 = new XElement("Food");
                                            xfood2 = new XElement("Food");
                                            xfood1.Add(new XElement("Name", new XAttribute("language", "ko"), new XAttribute("Value", "[이름 미등록]")));
                                            xfood2.Add(new XElement("Name", new XAttribute("language", "ko"), new XAttribute("Value", "[이름 미등록]")));
                                            xfood1.Add(new XElement("Name", new XAttribute("language", "en-US"), new XAttribute("Value", splitted[0])));
                                            if (splitted.Length > 1)
                                                xfood2.Add(new XElement("Name", new XAttribute("language", "en-US"), new XAttribute("Value", splitted[1])));
                                            else
                                                xfood2.Add(new XElement("Name", new XAttribute("language", "en-US"), new XAttribute("Value", "[name unregistered]")));
                                            break;
                                        }
                                    case 'K':
                                        {
                                            xfoods1.Add(xfood1);
                                            xfoods2.Add(xfood2);
                                            xfood1 = new XElement("Food");
                                            xfood2 = new XElement("Food");
                                            xfood1.Add(new XElement("Name", new XAttribute("language", "ko"), new XAttribute("Value", splitted[0])));
                                            if (splitted.Length > 1)
                                                xfood2.Add(new XElement("Name", new XAttribute("language", "ko"), new XAttribute("Value", splitted[1])));
                                            else
                                                xfood2.Add(new XElement("Name", new XAttribute("language", "ko"), new XAttribute("Value", "[이름 미등록]")));
                                            break;
                                        }
                                }
                                break;
                            case 'K':
                                switch (PresentLineLanguage)
                                {
                                    case 'E':
                                        {
                                            xfood1.Add(new XElement("Name", new XAttribute("language", "en-US"), new XAttribute("Value", splitted[0])));
                                            if (splitted.Length > 1)
                                                xfood2.Add(new XElement("Name", new XAttribute("language", "en-US"), new XAttribute("Value", splitted[1])));
                                            else
                                                xfood2.Add(new XElement("Name", new XAttribute("language", "en-US"), new XAttribute("Value", "[name unregistered]")));
                                            //xfoods.Add(xfood);
                                            //xfood = new XElement("Food");
                                            break;
                                        }
                                    case 'K':
                                        {
                                            xfood1.Add(new XElement("Name", new XAttribute("language", "en-US"), new XAttribute("Value", "[name unregistered]")));
                                            xfood2.Add(new XElement("Name", new XAttribute("language", "en-US"), new XAttribute("Value", "[name unregistered]")));
                                            xfoods1.Add(xfood1);
                                            xfoods2.Add(xfood2);
                                            xfood1 = new XElement("Food");
                                            xfood2 = new XElement("Food");
                                            xfood1.Add(new XElement("Name", new XAttribute("language", "ko"), new XAttribute("Value", splitted[0])));
                                            if (splitted.Length > 1)
                                                xfood2.Add(new XElement("Name", new XAttribute("language", "ko"), new XAttribute("Value", splitted[1])));
                                            else
                                                xfood2.Add(new XElement("Name", new XAttribute("language", "ko"), new XAttribute("Value", "[이름 미등록]")));
                                            break;
                                        }
                                }
                                break;
                        }
                        FormerLineLanguage = PresentLineLanguage;

                        if (processed.Length > 0)
                        {
                            //앞에서 뺀 괄호 다시 더함
                            Int32 splitpoint = str.IndexOf('/');
                            XElement langToBeFixed;
                            if (splitpoint < bdata.Value.StartPoint)
                                langToBeFixed = xfood1.Elements().Last();
                            else
                                langToBeFixed = xfood2.Elements().Last();

                            XAttribute fixstr = langToBeFixed.Attribute("Value");
                            fixstr.Remove();
                            langToBeFixed.Add(new XAttribute("Value", (String)fixstr + bdata.Value.ExtractedString));
                        }
                    }

                    if (xfood1.HasElements)
                        xfoods1.Add(xfood1);
                    if (xfood2.HasElements)
                        xfoods2.Add(xfood2);

                    var returner = new List<XElement>();
                    if (!IsMealBlank(xfoods2))
                        returner.Add(xfoods2);
                    if (!IsMealBlank(xfoods1))//D와 C의 순서가 이유는 몰라도 최근 바뀌었으므로 C보다 D를 앞으로.
                        returner.Add(xfoods1);
                    return returner.ToArray();
                }
                else if (IsThereNoType1)
                    return new XElement[] { ParseSingleFoodData(strfood, calint, type2) };
                else
                    return new XElement[] { ParseSingleFoodData(strfood, calint, type1) };
            }
            else
                return new XElement[0];
        }

        XElement ParseSingleFoodData(List<String> strfood, String calstr, String type)
        {
            XElement xfoods = new XElement("Foods", new XAttribute("Type", type));
            if (calstr.Length > 0)
                xfoods.Add(new XAttribute("Calories", calstr));
            else
                xfoods.Add(new XAttribute("Calories", -1));

            Char FormerLineLanguage = 'E';//K for Korean, E for English
            XElement xfood = null;//new XElement("Food");
            foreach (String str in strfood)
            {
                Nullable<BracketData> bdata = ExtractBracket(str);
                String processed = String.Empty;
                if (bdata.HasValue)
                {
                    processed = bdata.Value.AfterExtracted;
                    if (processed.Length == 0)
                    {
                        //마지막 코드에 괄호 추가, 더 할 일 없음. 바로 브레이크
                        XElement langToBeFixed = xfood.Elements().Last();
                        XAttribute fixstr = langToBeFixed.Attribute("Value");
                        fixstr.Remove();
                        langToBeFixed.Add(new XAttribute("Value", (String)fixstr + bdata.Value.ExtractedString));
                        break;
                    }
                }


                Char PresentLineLanguage;
                if (IsEnglish(str))
                    PresentLineLanguage = 'E';
                else
                    PresentLineLanguage = 'K';
                switch (FormerLineLanguage)
                {
                    case 'E':
                        switch (PresentLineLanguage)
                        {
                            case 'E':
                                xfoods.Add(xfood);
                                xfood = new XElement("Food");
                                xfood.Add(new XElement("Name", new XAttribute("language", "en-US"), new XAttribute("Value", str)));
                                break;
                            case 'K':
                                xfoods.Add(xfood);
                                xfood = new XElement("Food");
                                xfood.Add(new XElement("Name", new XAttribute("language", "ko"), new XAttribute("Value", str)));
                                break;
                        }
                        break;
                    case 'K':
                        switch (PresentLineLanguage)
                        {
                            case 'E':
                                xfood.Add(new XElement("Name", new XAttribute("language", "en-US"), new XAttribute("Value", str)));
                                break;
                            case 'K':
                                xfoods.Add(xfood);
                                xfood = new XElement("Food");
                                xfood.Add(new XElement("Name", new XAttribute("language", "ko"), new XAttribute("Value", str)));
                                break;
                        }
                        break;
                }
                FormerLineLanguage = PresentLineLanguage;

                if (processed.Length > 0)
                {
                    XElement langToBeFixed = xfood.Elements().Last();
                    XAttribute fixstr = langToBeFixed.Attribute("Value");
                    fixstr.Remove();
                    langToBeFixed.Add(new XAttribute("Value", (String)fixstr + bdata.Value.ExtractedString));
                }
            }

            if (xfood.HasElements)
                xfoods.Add(xfood);

            return xfoods;
        }

        struct BracketData
        {
            public Int32 StartPoint;
            public String AfterExtracted;
            public String ExtractedString;
        }

        /// <summary>
        /// String 안에 괄호가 있는지 검사하여 있으면 그에 대한 정보가 반환되며, 없을 경우 null이 반환됩니다.
        /// </summary>
        /// <param name="input">검사할 String</param>
        /// <returns>괄호의 시작점, 괄호를 포함해 괄호 안의 문자열, 또 그것을 제외한 문자열을 반환합니다.</returns>
        Nullable<BracketData> ExtractBracket(String input)
        {
            Int32 bracketStartPoint = input.IndexOf('(');
            if (bracketStartPoint != -1)
            {
                Int32 bracketEndPoint = input.LastIndexOf(')');
                Int32 Length = bracketEndPoint - bracketStartPoint + 1;
                if (Length > 0)
                    return new BracketData()
                    {
                        StartPoint = bracketStartPoint,
                        AfterExtracted = input.Remove(bracketStartPoint, Length),
                        ExtractedString = input.Substring(bracketStartPoint, Length)
                    };
            }
            return null;
        }

        Boolean IsEnglish(String str)
        {
            foreach (Char c in str)
            {
                //if (!(c == '\u0020' // space
                //    || (c >= 0x0041 && c <= 0x005A) // english capital letter
                //    || (c >= 0x0061 && c <= 0x007A))) // english small letter
                if (c > 0x007F) // is it out of ASCII code - combines all punctuation marks and English letters.
                    return false;
            }
            return true;
        }

        String[] SplitIntoTwoBySlash(String str)
        {
            return str.Split(new Char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
        }

        String[] SplitIntoTwoBySlashWithException(String str)// results '쇠고기볶음밥', '삼계탕1/2' when splitting 쇠고기볶음밥/삼계탕1/2
        {
            List<String> splitted = SplitIntoTwoBySlash(str).ToList();
            for (Int32 i = splitted.Count - 1; i > 0; i--)
            {
                String firststr = splitted[i];
                Char first = firststr[0];
                if (first >= '0' && first <= '9')
                {
                    String laststr = splitted[i - 1];
                    Char last = laststr[laststr.Length - 1];
                    if (last >= '0' && first <= '9')
                    {
                        Int32 index = i;
                        splitted.RemoveAt(i);
                        splitted.RemoveAt(i - 1);
                        laststr += '/' + firststr;
                        splitted.Insert(index - 1, laststr);
                    }
                }
            }
            return splitted.ToArray();
        }

        Boolean IsMealBlank(XElement xfoods)//코너가 하나뿐일 땐 <D코너> 등으로 표시하다 요즘은 그마저도 표시 안 하길래 따로 필터링
        {
            if (xfoods.Attribute("Calories").Value != "-1")
                return false;
            if (!xfoods.HasElements)
                return true;

            var foodNames = xfoods.Element("Food").Elements("Name").ToList();
            if (foodNames[0].Attribute("Value").Value == "[이름 미등록]" && foodNames[1].Attribute("Value").Value == "[name unregistered]")
                return true;
            else return false;
        }
    }
}
