using System;
using System.Configuration;
using System.Reflection;

namespace JobManager
{
    public class JobSettings
    {
        public string BatchBlobStorageConnection; 
        public string BatchBlobStorageName;       
        public string BatchBlobSTorageUrl;        
        public string BatchBlobStorageKey;  
        
        public string BatchAccountUrl;            
        public string BatchAccountName;           
        public string BatchAccountKey;   
        
        public string PoolOsFamily;               
        public string PoolNodeVirtualMachineSize; 
        public int PoolTargetNodeCount;  

        public int RetryDeltaBackoff;   
        public int RetryMaxCount;          
        
        public bool ShouldDeleteJob;
        public string JobResourceContainerName;
        
        public static JobSettings FromAppSettings()
        {
            var settings = new JobSettings();

            foreach (var field in typeof(JobSettings).GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                var raw = ConfigurationManager.AppSettings[field.Name];

                if (field.FieldType == typeof(String))
                {
                    field.SetValue(settings, raw);
                }
                else if (field.FieldType == typeof(Int32))
                {
                    field.SetValue(settings, Int32.TryParse(raw, out var intValue) ? intValue : default(int));
                }
                else if (field.FieldType == typeof(Boolean))
                {
                    field.SetValue(settings, bool.TryParse(raw, out var boolValue) ? boolValue : default(bool));
                }
                else
                {
                    throw new NotSupportedException($"The type { field.FieldType.Name } cannot be parsed.");
                }

            }

            return settings;            
        }
    }
}
