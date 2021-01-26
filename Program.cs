using Avalonia;
using Avalonia.ReactiveUI;

using YAME;

// Initialization code. Don't use any Avalonia, third-party APIs or any
// SynchronizationContext-reliant code before AppMain is called: things aren't initialized
// yet and stuff might break.
BuildAvaloniaApp()
    .StartWithClassicDesktopLifetime(null);

// Avalonia configuration, don't remove; also used by visual designer.
static AppBuilder BuildAvaloniaApp()
    => AppBuilder.Configure<App>()
        .UsePlatformDetect()
        .LogToTrace()
        .UseReactiveUI();