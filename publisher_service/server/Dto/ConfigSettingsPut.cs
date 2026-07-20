
namespace Dto;

public class ConfigSettingsGet
{
    public string Mnemonics { get; set; }
    public string ServiceName { get; set; }
    public string OrgName { get; set; }
    public string ServiceCode { get; set; }
    public string TargetCode { get; set; }
    public string Region { get; set; }

    public ConfigSettingsGet(Repo.ConfigSettings config)
    {
        this.Mnemonics = config.Mnemonics;
        this.ServiceName = config.ServiceName;
        this.OrgName = config.OrgName;
        this.ServiceCode = config.ServiceCode;
        this.TargetCode = config.TargetCode;
        this.Region = config.Region;
    }

}

