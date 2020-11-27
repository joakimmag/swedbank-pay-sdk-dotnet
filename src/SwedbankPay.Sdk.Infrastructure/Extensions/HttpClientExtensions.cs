﻿using SwedbankPay.Sdk.Exceptions;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SwedbankPay.Sdk.Extensions
{
    public static class HttpClientExtensions
    {
        public static async Task<T> GetAsJsonAsync<T>(this HttpClient httpClient, Uri uri)
        {
            var apiResponse = await httpClient.GetAsync(uri);

            var responseString = await apiResponse.Content.ReadAsStringAsync();

            if (!apiResponse.IsSuccessStatusCode)
            {
                IProblemResponse problemResponseDto = null;
                if (!string.IsNullOrEmpty(responseString))
                {
                    problemResponseDto = JsonSerializer.Deserialize<ProblemResponseDto>(responseString).Map();
                }

                throw new HttpResponseException(
                    apiResponse,
                    problemResponseDto,
                    BuildErrorMessage(responseString, uri, apiResponse));
            }

            return JsonSerializer.Deserialize<T>(responseString, JsonSerialization.JsonSerialization.Settings);
        }

        internal static async Task<T> SendAndProcessAsync<T>(this HttpClient httpClient, HttpMethod httpMethod, Uri uri, object payload)
            where T : class
        {
            using var httpRequestMessage = new HttpRequestMessage(httpMethod, uri);

            if (payload != null)
            {
                var content = JsonSerializer.Serialize(payload, JsonSerialization.JsonSerialization.Settings);
                httpRequestMessage.Content = new StringContent(content, Encoding.UTF8, "application/json");
            }

            using var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);

            string BuildErrorMessage(string httpResponseBody)
            {
                return $"{httpRequestMessage.Method}: {httpRequestMessage.RequestUri} failed with error code {httpResponseMessage.StatusCode} using bearer token {httpClient.DefaultRequestHeaders?.Authorization?.Parameter}. Response body: {httpResponseBody}";
            }

            string httpResponseContent = string.Empty;
            try
            {
                httpResponseContent = await httpResponseMessage.Content.ReadAsStringAsync();
                if (!httpResponseMessage.IsSuccessStatusCode)
                {
                    if (string.IsNullOrEmpty(httpResponseContent))
                    {
                        var httpStatusCode = (int)httpResponseMessage.StatusCode;
                        var problem = new ProblemResponse(httpResponseContent,
                                                          httpResponseContent,
                                                          httpResponseContent,
                                                          new List<IProblemResponseItem>(),
                                                          httpStatusCode,
                                                          httpResponseContent,
                                                          httpResponseContent);
                        throw new HttpResponseException(httpResponseMessage, problem, BuildErrorMessage(httpResponseContent));
                    }

                    var problemResponseDto = JsonSerializer.Deserialize<ProblemResponseDto>(httpResponseContent, JsonSerialization.JsonSerialization.Settings).Map();
                    throw new HttpResponseException(
                        httpResponseMessage,
                        problemResponseDto,
                        BuildErrorMessage(httpResponseContent));
                }

                return JsonSerializer.Deserialize<T>(httpResponseContent, JsonSerialization.JsonSerialization.Settings);
            }
            catch (HttpResponseException ex)
            {
                ex.Data.Add(nameof(httpResponseContent), httpResponseContent);
                throw ex;
            }
        }

        private static string BuildErrorMessage(string httpResponseBody, Uri uri, HttpResponseMessage httpResponse)
        {
            return
                $"GET: {uri} failed with error code {httpResponse.StatusCode}. Response body: {httpResponseBody}";
        }

        public static Task<T> PostAsJsonAsync<T>(this HttpClient httpClient, Uri uri, object payload)
            where T : class
        {
            return httpClient.SendAndProcessAsync<T>(HttpMethod.Post, uri, payload);
        }

        public static Task<T> SendAsJsonAsync<T>(this HttpClient httpClient, HttpMethod httpMethod, Uri uri, object payload = null)
            where T : class
        {
            return httpClient.SendAndProcessAsync<T>(httpMethod, uri, payload);
        }
    }
}
