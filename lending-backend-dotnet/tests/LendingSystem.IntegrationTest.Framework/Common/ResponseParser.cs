using System.Text.Json;
using LendingSystem.WebApi.Controllers;

namespace LendingSystem.IntegrationTest.Framework.Common;

/// <summary>
/// 解析 Response stream
/// </summary>
public static class ResponseParser
{
   /// <summary>
   /// 將 Response 轉型成 TResult 
   /// </summary>
   /// <param name="response"></param>
   /// <typeparam name="TResult"></typeparam>
   /// <returns></returns>
   public static async Task<ApiResponse<TResult>?> ParseJsonAsync<TResult>(
      HttpResponseMessage response)
   {
      var stream = await response.Content.ReadAsStreamAsync();
      var body = await JsonSerializer.DeserializeAsync<ApiResponse<TResult>>(stream);
      return body;
   }
}