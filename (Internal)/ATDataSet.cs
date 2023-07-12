//using Mozart.Data.Entity;

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Text;
//using System.Threading.Tasks;

//namespace Mozart.SeePlan.Aleatorik
//{
//    public class IEntityDataSet { }

//    public class ATDataSet<T> : IEntityDataSet where T : IPropertyObject
//    {

//        public EntityTable<T> Table;
//        public Dictionary<string, EntityView<T>> Views;

//        public bool IsLoad = false;
//        public ATDataSet()
//        {
//            this.CreateTable();

//        }
//        private void CreateTable()
//        {
//            if (this.Table == null)
//                this.Table = new EntityTable<T>();
//        }

//        public void ImportRow(T row)
//        {
//            this.Table.ImportRow(row);
//            this.IsLoad = true;

//        }
//        public void ImportRange(IEnumerable<T> ranges)
//        {

//            this.CreateTable();
//            foreach (var i in ranges)
//                this.Table.ImportRow(i);

//            this.IsLoad = true;
//        }

//        public void RemoveRow(T row)
//        {
//            this.Table.Rows.Remove(row);
//        }

//        public void Reset()
//        {

//            this.IsLoad = false;

//            if (this.Table != null)
//                this.Table.Clear();

//            if (this.Views != null)
//                this.Views.Clear();
//        }

//        public IEnumerable<T> GetTable()
//        {
//            this.CreateTable();
//            return this.Table.Rows;
//        }

//        internal void CreateView(string tag, Expression<Func<T, dynamic>> keySelector)
//        {
//            if (this.Table == null)
//                this.Table = new EntityTable<T>();

//            if (this.Views == null)
//                this.Views = new Dictionary<string, EntityView<T>>();

//            string indexKey = KeyHelper.CreateIndexKey<T>(keySelector);

//            EntityView<T> view;

//            if (this.Views.TryGetValue(tag, out view) == false)
//            {
//                view = new EntityView<T>(this.Table, null, indexKey, IndexType.Hashtable);

//                this.Views.Add(tag, view);
//            }
//        }

//        internal IEnumerable<T> GetView(string tag, params object[] values)
//        {
//            EntityView<T> view;

//            if (this.Views.TryGetValue(tag, out view) == false)
//            {
//                return Enumerable.Empty<T>();
//            }

//            IEnumerable<T> var = null;
//            var = view.FindRows(values);
//            if (var.Count() > 0)
//                return var;

//            return Enumerable.Empty<T>();
//        }

//        public IEnumerable<T> GetView(Expression<Func<T, dynamic>> keySelector, params object[] keys)
//        {
//            string indexKey = KeyHelper.CreateIndexKey<T>(keySelector);

//            return this.GetTableImpl(indexKey, keys);

//        }
//        //public IEnumerable<T> GetView(Dictionary<string, dynamic> keySelector)
//        //{
//        //    var keys = keySelector.Keys.OrderBy(x => x);

//        //    List<dynamic> values = new List<dynamic>();

//        //    foreach (string key in keys)
//        //        values.Add(keySelector[key]);

//        //    string indexKey = KeyHelper.CreateIndexKey(keys.ToArray());
//        //    return this.GetTableImpl(indexKey, values.ToArray());
//        //}

//        public IEnumerable<T> GetView(Func<T, bool> predicate)
//        {
//            if (this.Table == null)
//                return Enumerable.Empty<T>();

//            return this.Table.Where(predicate);
//        }

//        public IEnumerable<T> GetView(Expression<Func<T, dynamic>> keySelector, Func<T, bool> predicate, params object[] keys)
//        {
//            var selectData = this.GetView(keySelector, keys);

//            return selectData.Where(predicate);

//        }

//        private IEnumerable<T> GetTableImpl(string indexKey, params dynamic[] values)
//        {
//            if (this.Table == null)
//                return Enumerable.Empty<T>();

//            if (this.Views == null)
//                this.Views = new Dictionary<string, EntityView<T>>();

//            EntityView<T> view;

//            if (this.Views.TryGetValue(indexKey, out view) == false)
//            {
//                view = new EntityView<T>(this.Table, null, indexKey, IndexType.Hashtable);

//                this.Views.Add(indexKey, view);

//            }
//            IEnumerable<T> var = null;
//            var = view.FindRows(values);
//            if (var.Count() > 0)
//                return var;

//            return Enumerable.Empty<T>();

//        }
//    }

//    ////InputDataSet.cs

//    //public class InputDataSet<T> : EntityDataSet<T> where T : IEntityObject

//    //{
//    //    ModelContextBase ModelCtx;

//    //    public InputDataSet(ModelContextBase modelCtx)

//    //    {

//    //        this.ModelCtx = modelCtx;

//    //        Type type = typeof(T);

//    //        this.Table = this.ModelCtx.GetTable<T>(type.Name).ToEntityTable();

//    //    }

//    //}
//    ////OutputDataSet.cs

//    //public class OutputDataSet<T> : EntityTableSet<T> where T : IEntityObject

//    //{

//    //    private ResultContextBase RsltCtx;

//    //    public OutputDataSet(ResultContextBase rsltCtx)

//    //    {
//    //        this.RsltCtx = rsltCtx;

//    //        Type type = typeof(T);

//    //        this.Table = this.RsltCtx.GetTable<T>(type.Name).ToEntityTable();

//    //    }
//    //}
//    ////UIDataSet.cs

//    //public class UIDataSet<T> : EntityDataSet<T> where T : IEntityObject

//    //{

//    //    private IDataContext UiCtx;

//    //    public UIDataSet(IDataContext uiCtx)

//    //    {
//    //        this.UiCtx = uiCtx;

//    //    }
//    //    public IEnumerable<T> Query(ListDictionary args, bool queryOnce = false)

//    //    {
//    //        if (queryOnce)

//    //        {
//    //            return this.UiCtx.LoadQueryInput<T>(typeof(T).Name, args);

//    //        }

//    //        else

//    //        {
//    //            if (this.Table == null)

//    //                this.Table = this.UiCtx.LoadQueryInput<T>(typeof(T).Name, args).ToEntityTable();

//    //            return this.Table.Rows;

//    //        }

//    //    }
//    //}
//}
