using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.Entity;
using System.Linq;

namespace SourceLog.Model.Tests
{
	public class FakeDbSet<T> : IDbSet<T> where T : class
	{
		readonly HashSet<T> _data;
		readonly IQueryable _query;

		public FakeDbSet()
		{
			_data = new HashSet<T>();
			_query = _data.AsQueryable();
		}

		public virtual T Find(params object[] keyValues)
		{
			throw new NotImplementedException("Derive from FakeDbSet<T> and override Find");
		}

		T IDbSet<T>.Add(T entity)
		{
			_data.Add(entity);
			return entity;
		}

		public void Add(T entity)
		{
			_data.Add(entity);
		}

		T IDbSet<T>.Remove(T entity)
		{
			throw new NotImplementedException();
		}

		T IDbSet<T>.Attach(T entity)
		{
			throw new NotImplementedException();
		}

		public T Create()
		{
			throw new NotImplementedException();
		}

		public TDerivedEntity Create<TDerivedEntity>() where TDerivedEntity : class, T
		{
			throw new NotImplementedException();
		}

		public ObservableCollection<T> Local { get; set; }

		Type IQueryable.ElementType
		{
			get { return _query.ElementType; }
		}
		System.Linq.Expressions.Expression IQueryable.Expression
		{
			get { return _query.Expression; }
		}
		IQueryProvider IQueryable.Provider
		{
			get { return _query.Provider; }
		}

		System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
		{
			return _data.GetEnumerator();
		}
		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			return _data.GetEnumerator();
		}
	}
}
