using System;
using System.Collections.Generic;
using System.Text;

namespace CKLunchBot.Core.Menu
{
	public enum Restaurants
	{
		/// <summary>
		/// 다른 항목과 일치하지 않는 나머지 이름
		/// </summary>
		Unknown,
		/// <summary>
		/// 다반
		/// </summary>
		Daban,
		/// <summary>
		/// 난카츠난우동
		/// </summary>
		NankatsuNanUdong,
		/// <summary>
		/// 탕&찌개차림
		/// </summary>
		TangAndJjigae,
		/// <summary>
		/// 육해밥
		/// </summary>
		YukHaeBab,
		/// <summary>
		/// 기숙사 아침
		/// </summary>
		DormBreakfast,
		/// <summary>
		/// 기숙사 점심
		/// </summary>
		DormLunch,
		/// <summary>
		/// 기숙사 저녁
		/// </summary>
		DormDinner
	}
}
