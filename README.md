# Rapidity.Json

#### 基于表达式树实现的JSON解析器

核心类：
 - JsonReader
 - JsonWriter
 - JsonSerializer
    - IConverterCreator
    - TypeConverter
          
          - ValueConverter
          - ObjectConverter
          - EnumerableConverter
          - DictionaryConverter
          - ...
 
 - JsonToken
    - JsonObject
    - JsonArray
    - JsonString
    - JsonNumber
    - JsonBoolean
    - JsonNull
