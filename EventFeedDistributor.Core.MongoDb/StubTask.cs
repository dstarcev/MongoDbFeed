using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using MongoDB.Driver;

namespace EventFeedDistributor.Core.MongoDb {
	internal static class StubTask {
		static StubTask() {
			Void = Task.FromResult<object>(null);
		}

		public static Task Void {
			get; private set;
		}

		public static Task<T> Value<T>(T value) {
			return Task.FromResult(value);
		}
	}

	internal static class WriteConcernResultExtensions {
		public static WriteConcernResult ThrowIfNotOk(this WriteConcernResult result) {
			if (result == null) {
				return null;
			}

			if (!result.Ok) {
				throw new MongoException(result.ErrorMessage);
			}

			return result;
		}

		public static IEnumerable<WriteConcernResult> ThrowIfNotOk(this IEnumerable<WriteConcernResult> results) {
			if (results == null) {
				return null;
			}

			var exceptions = results.Where(r => !r.Ok).Select(r => new MongoException(r.ErrorMessage)).ToList();
			if (exceptions.Count > 0) {
				throw new AggregateException(exceptions);
			}

			return results;
		}
	}
}