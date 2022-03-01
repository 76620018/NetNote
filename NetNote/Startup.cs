using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using NetNote.Middleware;
using NetNote.Models;
using NetNote.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetNote
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
            string conn = Configuration["ConnectionStrings:NoteDB"];
            //Models.NoteContext.connectiongString = conn;
            services.AddDbContext<Models.NoteContext>(options => options.UseSqlServer(conn));

            services.AddIdentity<NoteUser, IdentityRole>()
                .AddEntityFrameworkStores<NoteContext>()
                .AddDefaultTokenProviders();
            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequiredLength = 6;
            });

            services.AddScoped<INoteTypeRepository, NoteTypeRepository>();
            services.AddScoped<INoteRepository, NoteRepository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            InitData(app.ApplicationServices);
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();

            //app.UseBasicMiddleware(new BasicUser
            //{
            //    UserName = "admin",
            //    Password = "123456"
            //});

            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Note}/{action=Index}/{id?}");
            });
        }
        private void InitData(IServiceProvider serviceProvider)
        {
            using (var serviceScope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                //获取注入的NoteContext
                var db = serviceScope.ServiceProvider.GetRequiredService<NoteContext>();
                db.Database.EnsureCreated();//如果数据库不存在则创建，存在则不做操作
                if (!db.NoteTypes.Any())//不存在类型则添加
                {
                    var notetypes = new List<NoteType>{
                        new NoteType{ Name="日常记录"},
                        new NoteType{ Name="代码收藏"},
                        new NoteType{ Name="消费记录"},
                        new NoteType{ Name="网站收藏"}
                    };
                    db.NoteTypes.AddRange(notetypes);
                    db.SaveChanges();
                }
                if (!db.Users.Any()) //不存在用户，默认添加admin用户
                {
                    var userManager = serviceScope.ServiceProvider.GetRequiredService<UserManager<NoteUser>>();
                    var noteUser = new NoteUser { UserName = "admin", Email = "admin@qq.com" };
                    userManager.CreateAsync(noteUser, "a123456").Wait();
                }
            }
        }
    }
}
