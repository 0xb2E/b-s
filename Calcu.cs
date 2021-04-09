using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Excel;
using System.Data;
using System.IO;
using UnityEngine.UI;
using UnityEngine.SceneManagement;//使用场景管理器









public class Calcu : MonoBehaviour
{

    #region -- 变量定义
    public Text text;
    public GameObject loadscreen;
    public Slider slider;
    public InputField input_len;
    public InputField input_wid;
    public InputField input_interval;
    public GameObject r;
    List<int> Cstate = new List<int> { 2 ,1 };
    List<int> pallet = new List<int> { 200, 200 };//托盘尺寸
    List<List<int>> orderlist = new List<List<int>>();//订单信息
    List<List<int>> palletlist = new List<List<int>>();//供箱序列
    List<List<int>> areatworesult = new List<List<int>>();//{方案箱子数，箱子1~n在orderlist中的序号，箱子1~n货物旋转因子，指标最优值}
    List<List<int>> areathreeresult = new List<List<int>>();//{方案箱子数，箱子1~n在orderlist中的序号，箱子1~n货物旋转因子，指标最优值}
    List<List<int>> areafourresult = new List<List<int>>();//{方案箱子数，箱子1~n在orderlist中的序号，箱子1~n货物旋转因子，指标最优值}
    List<List<int>> areafiveresult = new List<List<int>>();//{方案箱子数，箱子1~n在orderlist中的序号，箱子1~n货物旋转因子，指标最优值}
    List<List<int>> areasixresult = new List<List<int>>();//{方案箱子数，箱子1~n在orderlist中的序号，箱子1~n货物旋转因子，指标最优值}
    List<List<int>> areasevenresult = new List<List<int>>();//{方案箱子数，箱子1~n在orderlist中的序号，箱子1~n货物旋转因子，指标最优值}
    List<List<int>> result = new List<List<int>>(); // 存放结果集 
    List<int> temp = new List<int>(); // 符合条件的结果  k：题目中要求k个数的集合。 startIndex：下一层for循环搜索的起始位置。
    private string filepath;
    int interval = 2;//间隙
    #endregion


    void Start()
    {

    }
    void Update()
    {
        if(loadscreen.activeSelf==true)
        {
            loadscreen.GetComponent<Image>().color = colorful(loadscreen.GetComponent<Image>().color);
        }
        if (loadscreen.activeSelf == false)
        {
            if (input_wid.text.Length != 0)
            {
                if (input_len.text.Length == 0) GameObject.Find("wid1CM").GetComponent<TextMesh>().text = input_wid.text + "CM";
                else
                {
                    GameObject.Find("len1CM").GetComponent<TextMesh>().text = Mathf.Max(int.Parse(input_len.text), int.Parse(input_wid.text)).ToString() + "CM";
                    GameObject.Find("wid1CM").GetComponent<TextMesh>().text = Mathf.Min(int.Parse(input_len.text), int.Parse(input_wid.text)).ToString() + "CM";
                }
            }
            else if (input_len.text.Length != 0) GameObject.Find("len1CM").GetComponent<TextMesh>().text = input_len.text + "CM";

            if (input_interval.text.Length != 0) GameObject.Find("interval (1)").GetComponent<TextMesh>().text = input_interval.text + "CM";
        }
    }

     Color colorful(Color a)
    {if (Mathf.Abs ( a[Cstate[0]]- (Cstate[1] + 1) % 2 )<=0.01) { a[Cstate[0]]=Cstate[1] = (Cstate[1] + 1) % 2;Cstate[0] = (Cstate[0] + 1) % 3;  return a; }
        a[Cstate[0]] = Mathf.Lerp(a[Cstate[0]], (Cstate[1]+1)%2, 2f * Time.deltaTime);
        return a;
    }
    void chooseKfromM(int m, int k, int startIndex)
    {
        if (m < k) return;
        if (temp.Count == k)
        {
            List<int> temp1 = new List<int>(temp);
            result.Add(temp1);
            return;
        }
        for (int i = startIndex; i < m; i++)
        {

            temp.Add(i); // 处理
            chooseKfromM(m, k, i + 1);
            temp.RemoveAt(temp.Count - 1); // 回溯
        }
    }

    void combination(int k)
    {
        result.Clear(); // 可以不加
        temp.Clear();   // 可以不加
        chooseKfromM(orderlist.Count, k, 0);
    }
    #region -- 系统函数
    /*
    提供修改文件路径接口 
    */
    public void setpath(string path)
    {
        filepath = path;
    }
    /*
    将订单excel文件中数据读取至a二维List中储存 
    */
    private void getexcel(List<List<int>> a)
    {
        DataRowCollection _dataRowCollection = ReadExcel(filepath);
        //这里从 1 开始循环，因为第一行被表头占据了。所以具体解析数据的时候需要根据具体情况来定。
        a.Clear();
        for (int i = 1; i < _dataRowCollection.Count; i++)
        {
            a.Add(new List<int> { int.Parse(_dataRowCollection[i][0].ToString()), int.Parse(_dataRowCollection[i][1].ToString()), int.Parse(_dataRowCollection[i][2].ToString()) });
        }
        if (input_wid.text.Length != 0) pallet[1] = int.Parse(input_wid.text);
        if (input_len.text.Length != 0) pallet[0] = int.Parse(input_len.text);
        if (pallet[0] < pallet[1]) { pallet[0] += pallet[1];pallet[1] = pallet[0] - pallet[1];pallet[0] -= pallet[1]; }
        if (input_interval.text.Length != 0) interval = int.Parse(input_interval.text);
    }
    

    #region -- 自定义函数
    /// <summary>
    /// 读取 Excel 表并返回一个 DataRowCollection 对象
    /// </summary>
    /// <param name="_path">Excel 表路径</param>
    /// <param name="_sheetIndex">读取的 Sheet 索引。Excel 表中是有多个 Sheet 的</param>
    /// <returns></returns>
    private static DataRowCollection ReadExcel(string _path, int _sheetIndex = 0)
    {
        FileStream stream = File.Open(_path, FileMode.Open, FileAccess.Read, FileShare.Read);
        //IExcelDataReader excelReader = ExcelReaderFactory.CreateBinaryReader(stream);//读取 Excel 1997-2003版本
        IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);//读取 2007及以后的版本
        DataSet result = excelReader.AsDataSet();
        return result.Tables[_sheetIndex].Rows;
    }
    /// <summary>
    /// 读取 Excel 表并返回一个 DataRowCollection 对象
    /// </summary>
    /// <param name="_path">Excel 表路径</param>
    /// <param name="_sheetIndex">读取的 Sheet 名称。Excel 表中是有多个 Sheet 的</param>
    /// <returns></returns>
    private static DataRowCollection ReadExcel(string _path, string _sheetName)
    {
        FileStream stream = File.Open(_path, FileMode.Open, FileAccess.Read, FileShare.Read);
        //IExcelDataReader excelReader = ExcelReaderFactory.CreateBinaryReader(stream);//读取 Excel 1997-2003版本
        IExcelDataReader excelReader = ExcelReaderFactory.CreateOpenXmlReader(stream);//读取 2007及以后的版本
        DataSet result = excelReader.AsDataSet();
        return result.Tables[_sheetName].Rows;
    }
    #endregion

    private int peak(int a, int b)
    {
        return a > b ? a - b : b - a;
    }

    /*
    计算指标：“长宽冗余”  输出数组={最小值，前者旋转，后者旋转}---------------2
    */
    List<int> testput2(List<int> a, List<int> b, int len, int height)
    {
        int i, j, Min = int.MaxValue, mini = 0, minj = 0;
        List<int> rs = new List<int>();
        rs.Clear(); // 可以不加
        for (i = 1; i <= 2; i++)
            for (j = 1; j <= 2; j++)
            {

                if ((a[i] + b[j]) <= len - 2 * interval && Mathf.Max(a[3 - i], b[3 - j]) <= height)
                {
                    if (Min > len - a[i] - b[j] + peak(a[3 - i], b[3 - j]))
                    {
                        Min = len - a[i] - b[j] + peak(a[3 - i], b[3 - j]);
                        mini = i;
                        minj = j;
                    }
                    else if (Min == len - a[i] - b[j] + peak(a[3 - i], b[3 - j]) && a[i] + b[j] > a[mini] + b[minj])
                    {
                        mini = i;
                        minj = j;
                    }
                }
            }
        if (mini != 0 && minj != 0)
        {
            rs = new List<int> { Min, mini, minj };
        }

        return rs;
    }


    /*
    计算指标：三箱装入后“长宽冗余”  输出数组={最小值，前者旋转，后者旋转}---------------2
    */
    List<int> testput3(List<int> a, List<int> b, List<int> c, int len, int height)
    {
        int i, j, k, Min = int.MaxValue, mini = 0, minj = 0, mink = 0;
        List<int> rs = new List<int>();
        rs.Clear();
        for (i = 1; i <= 2; i++)
            for (j = 1; j <= 2; j++)
                for (k = 1; k <= 2; k++)
                {

                    if ((a[i] + b[j] + c[k]) <= len - 3 * interval && Mathf.Max(a[3 - i], b[3 - j], c[3 - k]) <= height)
                    {
                        if (Min > len - a[i] - b[j] - c[k] + Mathf.Max(a[3 - i], b[3 - j], c[3 - k]) - Mathf.Min(a[3 - i], b[3 - j], c[3 - k]))
                        {
                            Min = len - a[i] - b[j] - c[k] + Mathf.Max(a[3 - i], b[3 - j], c[3 - k]) - Mathf.Min(a[3 - i], b[3 - j], c[3 - k]);
                            mini = i;
                            minj = j;
                            mink = k;
                        }
                        else if (Min == len - a[i] - b[j] - c[k] + Mathf.Max(a[3 - i], b[3 - j], c[3 - k]) - Mathf.Min(a[3 - i], b[3 - j], c[3 - k]) && a[i] + b[j] + c[k] > a[mini] + b[minj] + c[mink])
                        {
                            mini = i;
                            minj = j;
                            mink = k;
                        }
                    }
                }
        if (mini != 0 && minj != 0 && mink != 0)
        {
            rs = new List<int> { Min, mini, minj, mink };
        }

        return rs;
    }


    /*
    计算指标：四箱装入后“长宽冗余”  输出数组={最小值，前者旋转，后者旋转}---------------2
    */
    List<int> testput4(List<int> a, List<int> b, List<int> c, List<int> d, int len, int height)
    {
        int i, j, k, l, Min = int.MaxValue, mini = 0, minj = 0, mink = 0, minl = 0;
        List<int> rs = new List<int>();
        rs.Clear(); // 可以不加
        for (i = 1; i <= 2; i++)
            for (j = 1; j <= 2; j++)
                for (k = 1; k <= 2; k++)
                    for (l = 1; l <= 2; l++)
                    {

                        if ((a[i] + b[j] + c[k] + d[l]) <= len - 4 * interval && Mathf.Max(a[3 - i], b[3 - j], c[3 - k], d[3 - l]) <= height)
                        {
                            if (Min > len - a[i] - b[j] - c[k] - d[l] + Mathf.Max(a[3 - i], b[3 - j], c[3 - k], d[3 - l]) - Mathf.Min(a[3 - i], b[3 - j], c[3 - k], d[3 - l]))
                            {
                                Min = len - a[i] - b[j] - c[k] - d[l] + Mathf.Max(a[3 - i], b[3 - j], c[3 - k], d[3 - l]) - Mathf.Min(a[3 - i], b[3 - j], c[3 - k], d[3 - l]);
                                mini = i;
                                minj = j;
                                mink = k;
                                minl = l;
                            }
                            else if (Min == (len - a[i] - b[j] - c[k] - d[l] + Mathf.Max(a[3 - i], b[3 - j], c[3 - k], d[3 - l]) - Mathf.Min(a[3 - i], b[3 - j], c[3 - k], d[3 - l])) && a[i] + b[j] + c[k] + d[l] > a[mini] + b[minj] + c[mink] + d[minl])
                            {
                                mini = i;
                                minj = j;
                                mink = k;
                                minl = l;
                            }
                        }
                    }
        if (mini != 0)
        {
            rs = new List<int> { Min, mini, minj, mink, minl };
        }

        return rs;
    }

    /*
*找到最小箱子
*/
    int findminbox()
    {
        int min = int.MaxValue;
        foreach (List<int> a in orderlist)
        {
            if (a[1] * a[2] < min) min = a[1] * a[2];
        }
        return min;

    }
    /*
    *遍历二维vec
    */
    void traversevector(List<List<int>> v)
    {
        if (v == null)
        {
            return;
        }

        foreach (List<int> attribute in v)
        {
            foreach (int b in attribute) { Debug.Log(b); }
        }

    }
    /*
    * 
    * 放置函数 作用：将订单序列序号为sequence的货物移到供箱序列中    sequence:箱子在订单中的编号 rotatefac:旋转因子
    */
    void settlev(int sequence, int rotatefac, int area)
    {
        if (orderlist == null) return;
        for (int i = 0; i < orderlist.Count; i++)
        {
            if (orderlist[i][0] == sequence)
            {
                palletlist.Add(new List<int> { area, orderlist[i][1], orderlist[i][2], sequence,rotatefac });
                orderlist.Remove(orderlist[i]);
            }
        }

    }

    /*
    *
    * 计算占用长度 作用：选择计算结果序列中计算占用长度
    */
    int calocclen(List<int> a)
    {
        int sumocclen = 0;
        for (int i = 1; i < a.Count / 2; i++)
        {
            sumocclen += orderlist[a[i]][a[i + a.Count / 2 - 1]];
        }
        return sumocclen;
    }
    /*
    *
    * 选择最优指标 作用：选择计算结果序列中最优组合并移到供箱序列中
    */
    void choosebest(List<List<int>> res, int area)
    {
        int min = int.MaxValue, num = 0, cnt = 0;
        foreach (List<int> group in res)
        {
            cnt++;
            if (group[group.Count - 1] < min || (group[group.Count - 1] == min && calocclen(group) > calocclen(res[num])))
            {
                min = group[group.Count - 1];
                num = cnt - 1; ;
            }
        }
        for (int i = res[num].Count / 2 - 1; i >= 1; i--)
        {
            settlev(orderlist[res[num][i]][0], res[num][i + res[num].Count / 2 - 1], area);
        }
        res.Clear();

    }

    void onepro() //第一区域处理
    {
        int max = 0, num = 0, rotate = 0;
        foreach (List<int> p in orderlist)
        {
            if (p[1] * p[2] > max && Mathf.Max(p[1], p[2]) <= pallet[0])
            {
                max = p[1] * p[2];
                num = p[0];
                rotate = p[1] >= p[2] ? 1 : 2;
            }
        }
        if (num != 0)
            settlev(num, 1, rotate);
    }

    /******
    第四区域开始
    anypro(最大占用长度，最大占用宽度，输出List，区域号);

        输出List参照choosebest格式
    *****/

    void anypro(int len, int wid, List<List<int>> arearesult, int areanumber)
    {
        if (orderlist.Count == 0) return;
        anyone(len, wid, arearesult);
        if (arearesult.Count == 1)
        {
            anytwo(len, wid, arearesult);
            if (arearesult.Count == 2)
            {
                anythree(len, wid, arearesult);
                if (arearesult.Count == 3)
                { anyfour(len, wid, arearesult); }
            }
        }
        if (arearesult.Count != 0) choosebest(arearesult, areanumber);
    }


    void anyone(int len, int wid, List<List<int>> arearesult)//第三区域仅放置一个箱子
    {
        int max = 0, num = 0, rotatef = 0, maxmansion = 0, cnt = 0;
        foreach (List<int> p in orderlist)
        {
            cnt++;
            if (Mathf.Min(p[1], p[2]) <= wid && (Mathf.Max(p[1], p[2]) - len + interval) <= 0)
            {
                if (Mathf.Max(p[1], p[2]) > max || (Mathf.Max(p[1], p[2]) == max && p[1] * p[2] > maxmansion))
                {
                    max = Mathf.Max(p[1], p[2]);
                    num = cnt - 1;
                    rotatef = p[1] >= p[2] ? 2 : 1;
                    maxmansion = p[1] * p[2];
                }
            }
            else continue;

        }
        if (rotatef != 0)
        {
            List<int> adj = new List<int> { 1, num, rotatef, len - max };
            arearesult.Add(adj);
        }
    }


    void anytwo(int len, int wid, List<List<int>> arearesult)//第二区域仅放置两个箱子  result={(0,1),(0,2),……}
    {
        int min = int.MaxValue, num = 0, mini = 0, minj = 0, lenocc = 0;
        int i;
        combination(2);

        for (i = 0; i < result.Count; i++)
        {
            int referrence = -1;

            List<int> sb = testput2(orderlist[result[i][0]], orderlist[result[i][1]], len - interval, wid - interval);
            if (sb.Count != 0)
            {
                referrence = sb[0];
            }
            if ((referrence < min && referrence >= 0) || (referrence == min && lenocc < orderlist[result[i][0]][mini] + orderlist[result[i][1]][minj]))
            {
                min = referrence;
                num = i;
                mini = sb[1];
                minj = sb[2];
                lenocc = orderlist[result[i][0]][mini] + orderlist[result[i][1]][minj];
            }

        }
        if (mini != 0 && minj != 0)
        {
            arearesult.Add(new List<int> { 2, result[num][0], result[num][1], 3 - mini, 3 - minj, min });
        }
    }


    void anythree(int len, int wid, List<List<int>> arearesult)
    {
        int min = int.MaxValue, num = 0, mini = 0, minj = 0, mink = 0, lenocc = 0;
        int i;
        combination(3);
        for (i = 0; i < result.Count; i++)
        {
            int referrence = -1;
            List<int> sb = testput3(orderlist[result[i][0]], orderlist[result[i][1]], orderlist[result[i][2]], len - interval, wid - interval);
            if (sb.Count != 0)
            {
                referrence = sb[0];
            }
            if ((referrence < min && referrence >= 0) || (referrence == min && lenocc < orderlist[result[i][0]][mini] + orderlist[result[i][1]][minj] + orderlist[result[i][2]][mink]))
            {
                min = referrence;
                num = i;
                mini = sb[1];
                minj = sb[2];
                mink = sb[3];
                lenocc = orderlist[result[i][0]][mini] + orderlist[result[i][1]][minj] + orderlist[result[i][2]][mink];
            }

        }
        if (min <= pallet[1] + pallet[0] && mini >= 0)
            arearesult.Add(new List<int> { 3, result[num][0], result[num][1], result[num][2], 3 - mini, 3 - minj, 3 - mink, min });
    }


    void anyfour(int len, int wid, List<List<int>> arearesult)
    {
        int min = int.MaxValue, num = 0, mini = 0, minj = 0, mink = 0, minl = 0, lenocc = 0;
        int i;
        combination(4);
        for (i = 0; i < result.Count; i++)
        {
            int referrence = -1;
            List<int> sb = testput4(orderlist[result[i][0]], orderlist[result[i][1]], orderlist[result[i][2]], orderlist[result[i][3]], len - interval, wid - interval);
            if (sb.Count != 0)
            {
                referrence = sb[0];
            }
            if ((referrence < min && referrence >= 0) || (referrence == min && lenocc < orderlist[result[i][0]][mini] + orderlist[result[i][1]][minj] + orderlist[result[i][2]][mink] + orderlist[result[i][3]][minl]))
            {
                min = referrence;
                num = i;
                mini = sb[1];
                minj = sb[2];
                mink = sb[3];
                minl = sb[4];
                lenocc = orderlist[result[i][0]][mini] + orderlist[result[i][1]][minj] + orderlist[result[i][2]][mink] + orderlist[result[i][3]][minl];
            }

        }
        if (min <= pallet[1] + pallet[0] && mini != 0)
            arearesult.Add(new List<int> { 4, result[num][0], result[num][1], result[num][2], result[num][3], 3 - mini, 3 - minj, 3 - mink, 3 - minl, min });
    }



    void handleL(int type, int up, int wid, int len, int side)
    {
        Debug.Log("type:" + type);
        Debug.Log("up:" + up);
        Debug.Log("wid:" + wid);
        Debug.Log("len:" + len);
        Debug.Log("side:" + side);

        List<List<int>> orderlistcopy = new List<List<int>>(orderlist);
        List<List<int>> orderlistcopy_6 = new List<List<int>>();
        List<List<int>> orderlistcopy_7 = new List<List<int>>();
        int index = palletlist.Count;
        if (orderlist.Count != 0 && palletlist.Count >= 1)
        {
            anypro(Mathf.Max(up, wid), Mathf.Min(up, wid), areasixresult, 6);
            orderlistcopy_6 = orderlist;

            orderlist = orderlistcopy;

            anypro(Mathf.Max(len, side), Mathf.Min(len, side), areasevenresult, 7);
            orderlistcopy_7 = orderlist;

            int mansion6 = 0, mansion7 = 0, maxwid = 0, flag = 0;
            if (palletlist.Count > index)
            {
                for (int i = index; i < palletlist.Count; i++)
                {

                    if (palletlist[i][0] == 6)
                    {
                        mansion6 += palletlist[i][1] * palletlist[i][2];
                    }
                    if (palletlist[i][0] == 7)
                    {
                        mansion7 += palletlist[i][1] * palletlist[i][2];
                    }
                }

                if (mansion6 >= mansion7)
                {
                    flag = up >= wid ? 1 : 2;
                    for (int i = palletlist.Count - 1; i >= index; i--)
                    {
                        if (palletlist[i][0] == 6)
                        {
                            if (palletlist[i][palletlist[i][palletlist[i].Count - 1]] > maxwid)
                            { maxwid = palletlist[i][palletlist[i][palletlist[i].Count - 1]]; }
                        }
                        if (palletlist[i][0] == 7)
                        {
                            palletlist.Remove(palletlist[i]);
                        }

                    }
                    orderlist = orderlistcopy_6;

                    GameObject.Find("palletlist_controller").GetComponent<Datalist>().SendMessage("writeareasixfacdata", new List<int> { type, 1, flag, palletlist.Count - index, maxwid });
                    if (flag == 1) handleL(type, up, wid - maxwid, len, side - maxwid);
                    else if (flag == 2) handleL(type, up - maxwid, wid, len, side);
                }
                else
                {
                    flag = len >= side ? 1 : 2;
                    for (int i = palletlist.Count - 1; i >= index; i--)
                    {

                        if (palletlist[i][0] == 6)
                        {
                            palletlist.Remove(palletlist[i]);
                        }
                        if (palletlist[i][0] == 7)
                        {

                            if (palletlist[i][palletlist[i][palletlist[i].Count - 1]] > maxwid)
                            { maxwid = palletlist[i][palletlist[i][palletlist[i].Count - 1]]; }

                            palletlist[i][0] = 6;
                        }
                        orderlist = orderlistcopy_7;

                    }

                    GameObject.Find("palletlist_controller").GetComponent<Datalist>().SendMessage("writeareasixfacdata", new List<int> { type, 2, flag, palletlist.Count - index, maxwid });
                    if (flag == 1) handleL(type, up, wid, len, side - maxwid);
                    else if (flag == 2) handleL(type, up - maxwid, wid, len - maxwid, side);
                }

            }
        }



    }

    public void next()
    {
        if (filepath == null) { r.SetActive(true); return; }
        loadscreen.SetActive(true); GameObject.Find("example").SetActive(false); StartCoroutine(ShowA());

    }
    private IEnumerator ShowA()
    {
        yield return new WaitForSecondsRealtime(0.1f);
        emerge();
    }

    public void emerge()
        {
            StartCoroutine(LoadScene());
            palletlist.Clear();
            getexcel(orderlist);
            //traversevector(orderlist);
            while (orderlist.Count != 0)
            {
                int index = palletlist.Count;
                onepro();
                anypro(pallet[1] - palletlist[0][1], pallet[1], areatworesult, 2);
                anypro(pallet[1] - palletlist[0][2], palletlist[0][1], areathreeresult, 3);
                int twowid = 0, threelen = 0, fourlen = 0, fivewid = 0;
                for (int i = index; i < palletlist.Count; i++)
                {
                    if (palletlist[i][0] == 2)
                    {
                        twowid = Mathf.Max(twowid, palletlist[i][palletlist[i][palletlist[i].Count - 1]]);
                    }
                }

                anypro(pallet[1] - twowid, pallet[0] - palletlist[0][1], areafourresult, 4);
                for (int i = index; i < palletlist.Count; i++)
                {
                    switch (palletlist[i][0])
                    {
                        case 3:
                            {
                                threelen = Mathf.Max(threelen, palletlist[i][palletlist[i][palletlist[i].Count - 1]]);
                                break;
                            }
                        case 4:
                            {
                                fourlen = Mathf.Max(fourlen, palletlist[i][palletlist[i][palletlist[i].Count - 1]]);
                                break;
                            }
                    }
                }

                anypro(pallet[0] - threelen - fourlen, Mathf.Min(pallet[1] - palletlist[0][2], pallet[1] - twowid), areafiveresult, 5);
                for (int i = index; i < palletlist.Count; i++)
                {
                    if (palletlist[i][0] == 5)
                    {
                        fivewid = Mathf.Max(fivewid, palletlist[i][palletlist[i][palletlist[i].Count - 1]]);
                    }
                }

                List<List<int>> orderlistcopy = new List<List<int>>(orderlist);
                List<List<int>> orderlistcopy_6 = new List<List<int>>();
                List<List<int>> orderlistcopy_7 = new List<List<int>>();
                int type = palletlist[0][2] >= twowid ? 1 : 2;

                handleL(type, pallet[0] - fourlen - threelen, type == 1 ? pallet[1] - fivewid - palletlist[0][2] : pallet[1] - fivewid - twowid, type == 1 ? pallet[0] - palletlist[0][1] - fourlen : palletlist[0][1] - threelen, type == 1 ? pallet[1] - fivewid - twowid : pallet[1] - fivewid - palletlist[0][2]);
            }
            //traversevector(orderlist);
            //traversevector(palletlist);
            GameObject.Find("palletlist_controller").GetComponent<Datalist>().SendMessage("writedatalist", palletlist);
            GameObject.Find("palletlist_controller").GetComponent<Datalist>().SendMessage("writepalletdata", pallet);
            GameObject.Find("palletlist_controller").GetComponent<Datalist>().SendMessage("writeinterval", interval);
            //SceneManager.LoadScene("show");
        }


        IEnumerator LoadScene()
        {
        float sb = 1.3f;
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync("show");

            asyncOperation.allowSceneActivation = false;

            while (!asyncOperation.isDone)
            {
            //Output the current progress
                text.text = (sb * 10) + "%";
                sb = (float)(sb * 1.05);
                slider.value = sb;

            if (asyncOperation.progress >= 0.9f)
            {

                slider.value = 1;

                text.text = "Press any key to continue";

                if (Input.anyKeyDown)
                { asyncOperation.allowSceneActivation = true; }
            }

            yield return null;
            }
        }
        #endregion
    }
