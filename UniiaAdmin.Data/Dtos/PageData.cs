namespace UniiaAdmin.Data.Dtos;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

public class PageData<T>
{
	public List<T> Items { get; set; } = new List<T>();

	public int TotalCount { get; set; }
}
