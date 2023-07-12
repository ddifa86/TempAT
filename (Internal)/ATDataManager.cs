//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Text;
//using System.Threading.Tasks;
//using Mozart.SeePlan.Aleatorik.Data;

//namespace Mozart.SeePlan.Aleatorik
//{
//    /// <summary>
//    /// IObject 형태의 Data를 관리하는 클래스
//    /// IDataObject를 Interface로 사용하는 클래스에 대해서만 다양한 View를 제공
//    /// </summary>
//    public class ATDataManager
//    {
//        private Dictionary<string, IEntityDataSet> DataMart = new Dictionary<string, IEntityDataSet>();


//        public ATDataManager()
//        {

//        }

//        private void Loading<T>(string key) where T : IPropertyObject
//        {
//            this.DataMart.Add(key, new ATDataSet<T>());
//        }


//        private ATDataSet<T> GetEntityDataSet<T>() where T : IPropertyObject
//        {
//            Type schemaType = typeof(T);
//            string key = schemaType.ToString() + "@" + schemaType.Name;

//            if (this.DataMart.ContainsKey(key) == false)
//                this.Loading<T>(key);

//            return this.DataMart[key] as ATDataSet<T>;
//        }

//        public void ImportRow<T>(T item) where T : IPropertyObject
//        {
//            Type schemaType = typeof(T);
//            string key = schemaType.ToString() + "@" + schemaType.Name;

//            if (this.DataMart.ContainsKey(key) == false)
//                this.Loading<T>(key);

//            (this.DataMart[key] as ATDataSet<T>).ImportRow(item);
//        }

//        public void RemoveRow<T>(T item) where T : IPropertyObject
//        {
//            Type schemaType = typeof(T);
//            string key = schemaType.ToString() + "@" + schemaType.Name;

//            if (this.DataMart.ContainsKey(key) == false)
//                this.Loading<T>(key);

//            (this.DataMart[key] as ATDataSet<T>).RemoveRow(item);
//        }

//        public IEnumerable<T> GetTable<T>() where T : IPropertyObject
//        {
//            var ds = this.GetEntityDataSet<T>();

//            return ds.GetTable();
//        }

//        public void CreateView<T>(string tag, Expression<Func<T, dynamic>> keySelector) where T : IPropertyObject
//        {
//            var ds = this.GetEntityDataSet<T>();

//            ds.CreateView(tag, keySelector);
//        }


//        public IEnumerable<T> GetView<T>(string tag, params object[] keys) where T : IPropertyObject
//        {

//            var ds = this.GetEntityDataSet<T>();

//            return ds.GetView(tag, keys);
//        }


//        public T GetFirstItem<T>(string tag, params object[] keys) where T : IPropertyObject
//        {
//            var ds = this.GetEntityDataSet<T>();

//            var result = ds.GetView(tag, keys);

//            return result.FirstOrDefault();
//        }




//        public IEnumerable<T> GetView<T>(Expression<Func<T, dynamic>> filter, params object[] keys) where T : IPropertyObject
//        {

//            var ds = this.GetEntityDataSet<T>();

//            return ds.GetView(filter, keys);

//        }

//        public IEnumerable<T> GetView<T>(Func<T, bool> predicate) where T : IPropertyObject
//        {
//            return this.GetEntityDataSet<T>().GetView(predicate);

//        }

//        public T GetFirstItem<T>(Expression<Func<T, dynamic>> filter, params object[] keys) where T : IPropertyObject
//        {
//            var result = GetView(filter, keys);

//            return result.FirstOrDefault();
//        }
//    }
//}
