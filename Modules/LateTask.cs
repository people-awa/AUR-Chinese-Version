using System;
using System.Runtime.CompilerServices;

namespace AmongUsRevamped;

class LateTask
{
    public string name;
    public float timer;
    public bool shouldLog;
    public Action action;

    public static List<LateTask> Tasks = [];

    public bool Run(float deltaTime)
    {
        timer -= deltaTime;

        if (timer <= 0)
        {
            action();
            return true;
        }

        return false;
    }

    public LateTask(
        Action action,
        float time,
        string name = "未命名任务",
        bool shoudLog = true,
        [CallerMemberName] string callerMethodName = "")
    {
        this.action = action;
        this.timer = time;

        if (name != "未命名任务")
            this.name = name;
        else
            this.name = callerMethodName + " 任务";

        this.shouldLog = shoudLog;

        Tasks.Add(this);

        if (name != "" && shoudLog)
            Logger.Info($"\"{name}\" 已创建", "延迟任务");
    }

    public static void Update(float deltaTime)
    {
        foreach (var task in Tasks.ToArray())
        {
            try
            {
                if (task.Run(deltaTime))
                {
                    if (task.name is not "" && task.shouldLog)
                        Logger.Info($"\"{task.name}\" 已完成", "延迟任务");

                    Tasks.Remove(task);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(
                    $"{ex.GetType()}: {ex.Message} 在任务 \"{task.name}\" 中发生错误\n{ex.StackTrace}",
                    "延迟任务错误",
                    false);

                Tasks.Remove(task);
            }
        }
    }
}