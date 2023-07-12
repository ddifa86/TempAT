using Mozart.Common;
using Mozart.Task.Execution;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mozart.SeePlan.Aleatorik
{
    public class CommonHelper
    {
        static Dictionary<string, int> _internalSequence = new Dictionary<string, int>();
        public static string GenerateID(string id)
        {
            if (_internalSequence.ContainsKey(id) == false)
                _internalSequence.Add(id, 0);

            var lotID = string.Format("{0}_{1}", id, _internalSequence[id]++);

            return lotID;
        }

        public static string CreateKey(params object[] args)
        {
            if (args == null || args.Count() == 0)
                return string.Empty;

            string key = string.Empty;

            foreach (var a in args)
            {
                if (a == null)
                    continue;

                if (string.IsNullOrEmpty(key))
                    key = a.ToString();

                else
                    key += "@" + a.ToString();
            }

            return key;
        }

//#if QD // 필요성 검토 후 삭제 필요
//        internal static void FileCopy(ModelTask task)
//        {
//            if (AleatorikGlobalParameters.Instance.ApplyFileCopy == false)
//                return;

//            Logger.MonitorInfo(string.Format("File Copy Start : {0 : yyyy-MM-dd HH:mm:ss}", DateTime.Now));

//            string sourceFolderPath = GetExpOutDir(task.Context);// task.Context.TaskContext.Get("#exp-outdir")?.ToString();

//            if (System.IO.Directory.Exists(sourceFolderPath) == false)
//            {
//                Logger.MonitorInfo(string.Format("[WARNING] Source folder path does not exist : {0}", sourceFolderPath));
//                return;
//            }

//            string destinationFolderPath = AleatorikGlobalParameters.Instance.DestinationFolderPath;

//            if (System.IO.Directory.Exists(destinationFolderPath) == false)
//            {
//                Logger.MonitorInfo(string.Format("[WARNING] Destination folder path does not exist : {0}", destinationFolderPath));
//                return;
//            }

//            string [] copyItems = AleatorikGlobalParameters.Instance.FileCopyItemNames.Split(',');

//            HashSet<string> copyItemList = new HashSet<string>();

//            foreach (var item in copyItems)
//                copyItemList.Add(item.Trim());

//            if (System.IO.Directory.Exists(sourceFolderPath))
//            {
//                string[] fileArray = System.IO.Directory.GetFiles(sourceFolderPath);

//                foreach (string file in fileArray)
//                {
//                    string fileName = System.IO.Path.GetFileName(file);

//                    try
//                    {
//                        string itemName = fileName.Split('.')[0];

//                        if (copyItemList.Contains(itemName) == false)
//                            continue;

//                        string destinationFileName = System.IO.Path.Combine(destinationFolderPath, fileName);

//                        System.IO.File.Copy(file, destinationFileName, true);
//                    }
//                    catch (Exception ex)
//                    {
//                        Logger.MonitorInfo(string.Format("[ERROR] FileName : {0} copy fail. ErrorMessage : {1}", fileName, ex.Message));
//                    }
//                }
//            }

//            Logger.MonitorInfo(string.Format("File Copy End : {0 : yyyy-MM-dd HH:mm:ss}", DateTime.Now));
//        }

//        private static string GetExpOutDir(ModelContext context)
//        {
//            if (context == null)
//                return null;

//            string outDir = null;

//            try
//            {
//                var dir = context.ModelDirectory;

//                var experiment = context.Arguments.GetValue<string>("#experiment", "Experiment 1");

//                var defaultOutPath = string.Format("{0}\\{1}\\{2}", dir, experiment, "Result 0");

//                outDir = defaultOutPath;
//            }
//            catch
//            {

//            }

//            return outDir;
//        }
//#endif
    }
}
