using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TPRestaurent.BackEndCore.Common.DTO.Response
{
    public class Compound
    {
        [JsonProperty("district")] public string District { get; set; }

        [JsonProperty("commune")] public string Commune { get; set; }

        [JsonProperty("province")] public string Province { get; set; }
    }

    public class MainTextMatchedSubstring
    {
        [JsonProperty("length")] public int Length { get; set; }

        [JsonProperty("offset")] public int Offset { get; set; }
    }

    public class MatchedSubstring
    {
        [JsonProperty("length")] public int Length { get; set; }

        [JsonProperty("offset")] public int Offset { get; set; }
    }

    public class PlusCode
    {
        [JsonProperty("compound_code")] public string CompoundCode { get; set; }

        [JsonProperty("global_code")] public string GlobalCode { get; set; }
    }

    public class Prediction
    {
        [JsonProperty("description")] public string Description { get; set; }

        [JsonProperty("matched_substrings")] public List<MatchedSubstring> MatchedSubstrings { get; set; }

        [JsonProperty("place_id")] public string PlaceId { get; set; }

        [JsonProperty("reference")] public string Reference { get; set; }

        [JsonProperty("structured_formatting")]
        public StructuredFormatting StructuredFormatting { get; set; }

        [JsonProperty("has_children")] public bool HasChildren { get; set; }

        [JsonProperty("plus_code")] public PlusCode PlusCode { get; set; }

        [JsonProperty("compound")] public CompoundInfo Compound { get; set; }

        [JsonProperty("terms")] public List<Term> Terms { get; set; }

        [JsonProperty("types")] public List<object> Types { get; set; }

        [JsonProperty("distance_meters")] public object DistanceMeters { get; set; }
    }

    public class AutoCompleteRoot
    {
        [JsonProperty("predictions")] public List<Prediction> Predictions { get; set; }

        [JsonProperty("execution_time")] public string ExecutionTime { get; set; }

        [JsonProperty("Status")] public string Status { get; set; }
    }

    public class StructuredFormatting
    {
        [JsonProperty("main_text")] public string MainText { get; set; }

        [JsonProperty("main_text_matched_substrings")]
        public List<MainTextMatchedSubstring> MainTextMatchedSubstrings { get; set; }

        [JsonProperty("secondary_text")] public string SecondaryText { get; set; }

        [JsonProperty("secondary_text_matched_substrings")]
        public List<object> SecondaryTextMatchedSubstrings { get; set; }
    }

    public class Term
    {
        [JsonProperty("offset")] public int Offset { get; set; }

        [JsonProperty("Value")] public string Value { get; set; }
    }

    public class CompoundInfo
    {
        public string District { get; set; }
        public string Commune { get; set; }
        public string Province { get; set; }
    }
}
