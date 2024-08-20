using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
public class ProcessKillList
{
    public List<string> ExactProcesses { get; set; }
    public List<string> ContainsProcesses { get; set; }
}

class Program
{
    static async Task Main(string[] args)
    {

        int memory = 0;

        Console.WriteLine(@"   _____          __  __ ______   _______ _    _ _____  ____   ____  
  / ____|   /\   |  \/  |  ____| |__   __| |  | |  __ \|  _ \ / __ \ 
 | |  __   /  \  | \  / | |__       | |  | |  | | |__) | |_) | |  | |
 | | |_ | / /\ \ | |\/| |  __|      | |  | |  | |  _  /|  _ <| |  | |
 | |__| |/ ____ \| |  | | |____     | |  | |__| | | \ \| |_) | |__| |
  \_____/_/    \_\_|  |_|______|    |_|   \____/|_|  \_\____/ \____/ 
                                                                     
                                                                     ");

        ProcessKillList processKillList = await GetProcessKillListAsync();

        // Kill les process par nom (exact)
        foreach (var processName in processKillList.ExactProcesses)
        {
            foreach (Process process in Process.GetProcessesByName(processName))
            {
                memory += Int32.Parse(process.WorkingSet64.ToString());
                Console.WriteLine(process.ProcessName + " (" + process.Id + ") ended");
                process.Kill();
            }
        }

        // Tuer les process qui contiennent un "mot clé"
        Process[] allProcesses = Process.GetProcesses();
        foreach (var process in allProcesses)
        {
            foreach (var keyword in processKillList.ContainsProcesses)
            {
                if (process.ProcessName.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    memory += Int32.Parse(process.WorkingSet64.ToString());
                    Console.WriteLine(process.ProcessName + " ("+process.Id+") ended");
                    process.Kill();
                    break;
                }
            }
        }

        Console.WriteLine("Cleared " + BytesToString(memory));

        Console.WriteLine(@" _             _ _                                     _     
| |__  _   _  | (_) __ _ _ __ ___   __ _  ___ _ __    (_)___ 
| '_ \| | | | | | |/ _` | '_ ` _ \ / _` |/ _ \ '_ \   | / __|
| |_) | |_| | | | | (_| | | | | | | (_| |  __/ | | |_ | \__ \
|_.__/ \__, | |_|_|\__,_|_| |_| |_|\__, |\___|_| |_(_)/ |___/
       |___/                       |___/            |__/     ");

        Console.ReadLine();
    }

    static async Task<ProcessKillList> GetProcessKillListAsync()
    {
        string url = "https://liamgenjs.vercel.app/api/game-turbo/list.json";
        ProcessKillList processKillList = new ProcessKillList();

        // Liste locale en cas d'échec de la requête
        ProcessKillList localProcessKillList = new ProcessKillList
        {
            ExactProcesses = new List<string>
            {
                "MSPCManager", "Lively", "PowerToys", "FanControl", "flux", "Razer Central",
                "Razer Synapse 3", "GameServiceManager", "Razer Central Service", "Everything",
                "TranslucentTB", "GoogleDriveFS", "Nextcloud", "Microsoft SharePoint",
                "Microsoft Office Click-to-Run (SxS)", "Microsoft Office Click-to-Run Service",
                "GameManagerService3", "GameManagerService3 (32 bits)", "TeamViewer",
                "Papercut.Service", "FileZilla Server", "Windows Subsystem for Linux Service",
                "Razer Chroma SDK Service Host", "Razer Chroma SDK Service Host (32 bits)"
            },
            ContainsProcesses = new List<string>
            {
                "Razer"
            }
        };

        using (HttpClient client = new HttpClient())
        {
            Console.WriteLine($"Fetching data from the remote server");
            try
            {
                HttpResponseMessage response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                processKillList = JsonConvert.DeserializeObject<ProcessKillList>(responseBody);
                Console.WriteLine($"Data from remote server loaded !");
            }
            catch (HttpRequestException e)
            {
                Console.WriteLine($"Error when requesting remote server : {e.Message}. Loaded local list.");
                // Utiliser la liste locale si la requête échoue
                processKillList = localProcessKillList;
            }
        }

        return processKillList;
    }

    static String BytesToString(long byteCount)
    {
        string[] suf = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
        if (byteCount == 0)
            return "0" + suf[0];
        long bytes = Math.Abs(byteCount);
        int place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
        double num = Math.Round(bytes / Math.Pow(1024, place), 1);
        return (Math.Sign(byteCount) * num).ToString() + suf[place];
    }
}
