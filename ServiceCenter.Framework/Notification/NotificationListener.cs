
namespace ServiceCenter.Framework.Notification
{
    public abstract class NotificationListener : Plugin
    {
        public abstract void Notify(NotificationContext context);

        public NotificationListener()
        {
            PluginType = typeof(NotificationListener).FullName;
        }
    }
}
