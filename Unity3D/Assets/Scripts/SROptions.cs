using System.ComponentModel;
using HomeResVerify;

public partial class SROptions
{
    [Category("通用")]
    [DisplayName("退出")]
    public void Exist()
    {
        RoomManager.Instance.Exit();
    }
    
    [Category("装修")]
    [DisplayName("打开Node列表")]
    public void OpenNodeList()
    {
        if(null != UIManager.Instance.GetUI<UIRoom>()) UIManager.Instance.OpenUI<UINodeList>();
    }
    
    [Category("装修")]
    [DisplayName("显示普通家具")]
    public void ViewNormal()
    {
        UIManager.Instance.GetUI<UIRoom>()?.ViewNormal();
    }
    
    [Category("装修")]
    [DisplayName("显示旧家具")]
    public void ViewOld()
    {
        UIManager.Instance.GetUI<UIRoom>()?.ViewOld();
    }
    
    [Category("装修")]
    [DisplayName("显示整体清理")]
    public void ViewClean()
    {
        RoomManager.Instance.room?.ViewWClean();
    }
    
    [Category("装修")]
    [DisplayName("关闭整体清理")]
    public void CloseClean()
    {
        RoomManager.Instance.room?.CloseClean();
    }
}