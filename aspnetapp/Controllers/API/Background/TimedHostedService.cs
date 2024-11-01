using aspnetapp.Controllers.API.Miniprogram;

namespace aspnetapp.Controllers.API.Background
{
    public class TimedHostedService : BackgroundService {
        private readonly IServiceProvider _serviceProvider;
        private Timer _timer; // 定时器

        public TimedHostedService(IServiceProvider serviceProvider) {
            _serviceProvider = serviceProvider;
        }

        // 执行任务
        protected override Task ExecuteAsync(CancellationToken stoppingToken) {
            _timer = new Timer(DoWork, null, TimeSpan.Zero, TimeSpan.FromMinutes(10)); // 每10分钟执行一次

            return Task.CompletedTask;
        }

        // 定时器具体执行的方法
        private async void DoWork(object state) {
            // 创建服务作用域
            using (var scope = _serviceProvider.CreateScope()) {
                var orderService = scope.ServiceProvider.GetRequiredService<OrderAPI>();
                // 检查订单是否已支付，超过10分钟未支付订单自动关闭
                await orderService.CheckOrderPayment();
            }
        }

        // 释放资源
        public override void Dispose() {
            _timer?.Dispose(); // 如果_timer对象不为null，则销毁
            base.Dispose();
        }
    }
}
