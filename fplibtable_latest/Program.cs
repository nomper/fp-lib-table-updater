using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;

// とってもこのhtmlファイル任せ(htmlもよう分からんし)

namespace ConsoleApplication1
{
    class Program
    {
        static bool flag;
        const bool dev = true;
        const int MAX_NUM_OF_LINES = 10002;  // この値以上の行数のhtmlファイルは無理(動的確保しろよとか知らない)
        //static int now_line;
        static string[] CacheResult = new string[1000];
//        static string[] CacheResult = new string[1000];
        public static Int32 p = 0;
        //static string targetfile = System.Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\kicad\\fp-lib-table";
        static string targetfile = "fp-lib-table";
        static void Main(string[] args)
        {
            onlineUpdate();
            // offlineUpdate();
            p = 0;
            Write_LF("\n\n出力するよ\n\n");
            Array.Sort(CacheResult);
            output();

            return;
        }

        // webから取得して出力
        static void onlineUpdate()
        {
            update(1);
            int numOfPages = getNumOfPages();
            
            //fp-lib-tableに出力

            for (int i = 1; i <= numOfPages; i++)
            {
                update(i);
                extract(i);
                File.Delete("page"+i+".html");
            }
        }

        // ローカルのhtmlファイルから出力
        static void offlineUpdate()
        {
            int numOfPages = getNumOfPages();

            for (int i = 1; i <= numOfPages; i++)
            {
                extract(i);
            }
        }

        // 文字列の出現回数取得
        static int CountString(string s, string text)
        {
            return (s.Length - s.Replace(text, "").Length)/text.Length; // 指定した文字列を削除して元の文字数と比較、そんで指定した文字列の長さで割ってます。
        }

        // CountString()関数を使ってページ数取得(てきとー)
        static int getNumOfPages()
        {
            StreamReader sr = new StreamReader("page1.html", Encoding.GetEncoding("Shift_JIS"));
            int numOfPage = CountString(sr.ReadToEnd(), "/KiCad?page=");    // これ指定したら都合よかったから、根拠はない
            Console.WriteLine("{0} pages.", numOfPage);
            sr.Close();
            return numOfPage;
        }

        // webから最新のhtmlファイルを取得
        static void update(int pageNum)
        {
            string outputHtml = string.Format("page{0}.html", pageNum);
            StreamWriter writer = new StreamWriter(outputHtml);
            WebClient client = new WebClient();
            string str = client.DownloadString(string.Format("https://github.com/KiCad?page={0}&tab=repositories", System.Uri.EscapeDataString(pageNum.ToString("d"))));
            writer.Write(str);
            writer.Close();

        }

        // ローカルのhtmlファイルから"*.pretty"を抽出(今コンソール表示のみ、これもてきとー)
        static void extract(int pageNum)
        {
            int numLines = 0;   // 行数
            System.IO.StreamReader cReader = (new System.IO.StreamReader(string.Format("page{0}.html", pageNum), System.Text.Encoding.Default));

            //一括格納
            string stResult = cReader.ReadToEnd();
            
            // cReader を閉じる (正しくは オブジェクトの破棄を保証する を参照)
            cReader.Close();
            Console.WriteLine("\nupdated page{0}!!\n", pageNum);
            if (System.Text.RegularExpressions.Regex.IsMatch(stResult, @"/KiCad/(\w)+.pretty"))
                {
                    Write_LF("matched");
                    System.Text.RegularExpressions.MatchCollection mc = System.Text.RegularExpressions.Regex.Matches(stResult, @"/KiCad/([\w]+).pretty");
                    foreach (System.Text.RegularExpressions.Match m in mc) {
                        for(int i = 0; i < 1000; i++)
                        {
                            flag = true;
                            // Write_LF(CacheResult[i]);
                            if (CacheResult[i] == m.Groups[1].Value)
                            {
                                flag = false;
                                break;
                            } 
                        }
                        if(flag) CacheResult[p++] = m.Groups[1].Value;
                        
                        Write_LF(m.Groups[1].Value);
                        

                    }
                }
            Console.WriteLine("\nextracted from page{0}!!\n", pageNum);
        }
        static void output()
        {
            write_table("(fp_lib_table", false);
            for (int i = 0; i < 1000; i++)
            {
                if (CacheResult[i] != null)
                {
                    Console.WriteLine("{0}", CacheResult[i]);
                    //write_table(CacheResult[i]);
                    write_table("  (lib (name " + CacheResult[i] + ")(type Github)(uri ${KIGITHUB}/" + CacheResult[i] + ".pretty)(options \"\")(descr \"\"))", true);
                }
            }
            write_table(")");
            Write_LF(targetfile + " に出力しました。");

        }
        static void write_table(string str, bool flag=true)
        {
            StreamWriter tableout = new StreamWriter(targetfile, flag, Encoding.GetEncoding("shift_jis"));
            //fp-lib-tableに出力
            tableout.WriteLine(str);
            tableout.Close();
        }
        static void Write_LF(string str)
        {
            Console.WriteLine(str);
#if dev
            StreamWriter tableout = new StreamWriter("dev.txt", true, Encoding.GetEncoding("shift_jis"));
            //fp-lib-tableに出力
            tableout.WriteLine(str);
            tableout.Close();
#endif
        }

    }
}