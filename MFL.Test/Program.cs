using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MFL.Core.ExpTreeCompiler;
using MFL.Core.Publish;
using MFL.Core.Publish.Model;
using MFL.Core.TokenParser;
using Newtonsoft.Json;
using MFL.Core.SyntaxParser;

namespace MFL.Test
{
    class Program
    {
        static void Main(string[] args)
        {
            var typemaper = TypePublisher.PublishTypeInfo<OrderInfo>();
            var typeJsonValue = JsonConvert.SerializeObject(typemaper);
            //string filter = "root.DishSample.DishName eq \"中文测试\" and root.CustomerCount gt 2  with root.Dishes(x=>x.DishName eq \"sasa\" with x.PracticeList(y=>y.Name eq \"sss\" and y.AddPrice gt 5 )) ,  root.Dishes2(x=>x.DishName eq \"%3323%\")   ";
            string filter = "root.DishSample.DishName eq \"中文测试\" and root.CustomerCount gt 3 with root.Dishes(x1 => x1.DishName eq \"sasa\" with x1.PracticeList(x2 => x2.Name eq \"sss\" and x2.AddPrice gt 10)), root.Dishes2(x1 => x1.DishName eq \"%3323%\")";
            var tokenlist = filter.GetTokens();
            var node = tokenlist.CreateExpTree();
            var ts = new ExpVisiter<OrderInfo>(node).StartVisit();
            var s = new OrderInfo()
            {
                DishSample = new Dish()
                {
                    DishName = "中文测试"
                },
                CustomerCount = 5,
                Dishes = new List<Dish>()
                {
                    new Dish(){ DishName = "123" },
                    new Dish(){ DishName = "sasa",
                        PracticeList = new List<DishPractice>()
                        {
                            new DishPractice(){ Id = 1, AddPrice = 10, Name = "sss"},
                            new DishPractice(){ Id = 1, AddPrice = 11, Name = "sss"},
                            new DishPractice(){ Id = 1, AddPrice = 10, Name = "ssssss"},
                        }}
                },
                Dishes2 = new List<Dish>()
                {
                    new Dish(){ DishName = "1233233"}
                }
            };
            var ttt = ts(s);
            Console.ReadLine();
        }
    }



    public class OrderInfo
    {
        [Desc("下单时间")]
        public string OrderTime;
        [Desc("份数")]
        public string TakeMealNum;
        [Desc("扫码时间")]
        public string Scene;
        [Desc("消费者ID")]
        public string CustomerName;
        [Desc("消费者手机号")]
        public string CustomerPhone;
        [Desc("就餐时间")]
        public string EatingTime;
        [Desc("就餐人数")]
        public int CustomerCount;
        [Desc("备注")]
        public string Remark;
        [Desc("单菜品测试数据")]
        public Dish DishSample;
        [Desc("菜品数据列表")]
        public List<Dish> Dishes;
        [Desc("菜品数据列表2")]
        public List<Dish> Dishes2;
    }


    public class Dish
    {
        [Desc("菜品名称")]
        public string DishName;
        [Desc("份数")]
        public double TakeCount;
        [Desc("菜品描述")]
        public string DishDescription;
        [Desc("菜品单价")]
        public double UnitPrice;
        [Desc("单位名称")]
        public List<string> PracticeNames;
        [Desc("子菜品信息")]
        public List<Dish> SubDishes;
        [Desc("菜品附加信息列表")]
        public List<DishPractice> PracticeList;
    }


    public class DishPractice
    {
        [Desc("ID信息")]
        public int Id;
        [Desc("Name信息")]
        public string Name;
        [Desc("加价")]
        public int AddPrice;
    }
}
