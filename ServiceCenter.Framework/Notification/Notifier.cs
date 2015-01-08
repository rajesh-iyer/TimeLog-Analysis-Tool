
namespace ServiceCenter.Framework.Notification
{
    public abstract class Notifier : Plugin
    {
        public abstract void Notify(NotificationContext context);

        public Notifier()
        {
            PluginType = typeof(Notifier).FullName;
        }
    }
}
