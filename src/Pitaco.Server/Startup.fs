namespace Pitaco.Server

open Microsoft.AspNetCore
open Microsoft.AspNetCore.Authentication.Cookies
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.DependencyInjection
open Bolero.Remoting.Server
open Bolero.Server
open Bolero.Templating.Server

type Startup() =

    member this.ConfigureServices(services: IServiceCollection) =
        services.AddCors(fun options -> options.AddPolicy("CorsPolicy", fun policy ->
            policy.AllowAnyMethod().AllowCredentials().AllowAnyHeader().AllowCredentials().SetIsOriginAllowed(fun x -> true) |> ignore
        )) |> ignore
        services.AddMvc() |> ignore
        services.AddServerSideBlazor() |> ignore
        services
            .AddAuthorization()
            .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie()
                .Services
            .AddRemoting<DashboardService>()
            .AddBoleroHost()
#if DEBUG
            .AddHotReload(templateDir = __SOURCE_DIRECTORY__ + "/../Pitaco.Client")
#endif
        |> ignore

    member this.Configure(app: IApplicationBuilder, env: IWebHostEnvironment) =
        app
            .UseCors("CorsPolicy")
            .UseAuthentication()
            .UseRemoting()
            .UseStaticFiles()
            .UseRouting()
            .UseBlazorFrameworkFiles()
            .UseEndpoints(fun endpoints ->
#if DEBUG
                endpoints.UseHotReload()
#endif
                endpoints.MapBlazorHub() |> ignore
                endpoints.MapFallbackToBolero(Index.page) |> ignore)
        |> ignore

module Program =

    [<EntryPoint>]
    let main args =
        WebHost
            .CreateDefaultBuilder(args)
            .UseStaticWebAssets()
            .UseStartup<Startup>()
            .Build()
            .Run()
        0
