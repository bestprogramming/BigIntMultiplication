# BigInt Multiplication
BigInt Multiplication provides an example for multiplying two 64-bit arrays. 
Performs a total of leftLength x rightLength multiplication operations.
It does not require the result array to be reset while the calculation is performed and sequential access is provided while reaching the result.
Performing multiplication sequentially may be more amenable to parallel computation in the future.
Especially if features like AVX, 64 bit multiplication and writing the result as low and high bits and addition with carry are included. Of course, loading the registers (ymm, zmm) from memory in reverse order can bring an extra performance increase.

## Decimal digits count of 64-bit arrays
When performing mathematical operations, we may need operations with higher precision than numbers such as double or float. 
For example, if we want to find the roots of a quartic function as a double, the input of the function must be greater than the double precision.

| Length | Byte Count | Decimal Count |
| :----- | :--------- | :------------ |
|  8     |  64        |  155          |
|  16    |  128       |  309          |
|  32    |  256       |  617          |
|  64    |  512       |  1234         |
|  128   |  1024      |  2467         |
|  256   |  2048      |  4933         |
|  512   |  4096      |  9865         |
|  1024  |  8192      |  19719        |
|  2048  |  16384     |  39447        |


## Calculation Process
For example, if leftLength is 8 and rightLength is 5, the order of operations is listed in the table below. 
Result Index is the index where the multiplication result will be written. 
Left Index, Right Index values are multiplied and each product is added and written to Result Index. 
Rows can be divided into 3 regions. Start [0-4], middle [5-7], finish [8, 11]. 
Thus, the code is run without making too many checks in loops.

|Result Index| Left Index, Right Index                                                  |
| :--------- | :------------------------------------------------------------------------|
|0           |&nbsp;&nbsp;&nbsp;0,0&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;|
|1           |&nbsp;&nbsp;&nbsp;0,1&nbsp;&nbsp;&nbsp;1,0&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;|
|2           |&nbsp;&nbsp;&nbsp;0,2&nbsp;&nbsp;&nbsp;1,1&nbsp;&nbsp;&nbsp;2,0&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;|
|3           |&nbsp;&nbsp;&nbsp;0,3&nbsp;&nbsp;&nbsp;1,2&nbsp;&nbsp;&nbsp;2,1&nbsp;&nbsp;&nbsp;3,0&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;|
|4           |&nbsp;&nbsp;&nbsp;0,4&nbsp;&nbsp;&nbsp;1,3&nbsp;&nbsp;&nbsp;2,2&nbsp;&nbsp;&nbsp;3,1&nbsp;&nbsp;&nbsp;4,0&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;|
|5           |&nbsp;&nbsp;&nbsp;&nbsp;X&nbsp;&nbsp;&nbsp;&nbsp;1,4&nbsp;&nbsp;&nbsp;2,3&nbsp;&nbsp;&nbsp;3,2&nbsp;&nbsp;&nbsp;4,1&nbsp;&nbsp;&nbsp;5,0&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;|
|6           |&nbsp;&nbsp;&nbsp;&nbsp;X&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;X&nbsp;&nbsp;&nbsp;&nbsp;2,4&nbsp;&nbsp;&nbsp;3,3&nbsp;&nbsp;&nbsp;4,2&nbsp;&nbsp;&nbsp;5,1&nbsp;&nbsp;&nbsp;6,0&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;|
|7           |&nbsp;&nbsp;&nbsp;&nbsp;X&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;X&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;X&nbsp;&nbsp;&nbsp;&nbsp;3,4&nbsp;&nbsp;&nbsp;4,3&nbsp;&nbsp;&nbsp;5,2&nbsp;&nbsp;&nbsp;6,1&nbsp;&nbsp;&nbsp;7,0&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;|
|8           |&nbsp;&nbsp;&nbsp;&nbsp;X&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;X&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;X&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;X&nbsp;&nbsp;&nbsp;&nbsp;4,4&nbsp;&nbsp;&nbsp;5,3&nbsp;&nbsp;&nbsp;6,2&nbsp;&nbsp;&nbsp;7,1&nbsp;&nbsp;&nbsp;&nbsp;X&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;|
|9           |&nbsp;&nbsp;&nbsp;&nbsp;X&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;X&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;X&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;X&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;X&nbsp;&nbsp;&nbsp;&nbsp;5,4&nbsp;&nbsp;&nbsp;6,3&nbsp;&nbsp;&nbsp;7,2&nbsp;&nbsp;&nbsp;&nbsp;X&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;X&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;|
|10          |&nbsp;&nbsp;&nbsp;&nbsp;X&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;X&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;X&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;X&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;X&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;X&nbsp;&nbsp;&nbsp;&nbsp;6,4&nbsp;&nbsp;&nbsp;7,3&nbsp;&nbsp;&nbsp;&nbsp;X&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;X&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;X&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;|
|11          |&nbsp;&nbsp;&nbsp;&nbsp;X&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;X&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;X&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;X&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;X&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;X&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;X&nbsp;&nbsp;&nbsp;&nbsp;7,4&nbsp;&nbsp;&nbsp;&nbsp;X&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;X&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;X&nbsp;&nbsp;&nbsp;&nbsp;&nbsp;X&nbsp;&nbsp;&nbsp;|

## Performance Results
In the performance table below, two random 64-bit numbers of length leftLength and rightLength were multiplied. 
Each line was run 300 times and the minimum ElapsedTicks values were written with StopWatch(.NET). 
ExpectedTick(max) belongs to BigInteger(.NET).

|leftLength          |rightLength         |expectedTick(max)   |actualTick          |Percent |
| :----------------- | :----------------- | :----------------- | :----------------- | :----- |
|16                  |16                  |9                   |7                   |77%     |
|16                  |32                  |18                  |12                  |66%	 |
|16                  |64                  |35                  |21                  |60%	 |
|16                  |128                 |70                  |39                  |55%	 |
|16                  |256                 |140                 |80                  |57%	 |
|16                  |512                 |278                 |139                 |50%	 |
|16                  |1024                |549                 |266                 |48%	 |
|16                  |2048                |1117                |582                 |52%	 |
|32                  |16                  |18                  |13                  |72%	 |
|32                  |32                  |30                  |24                  |80%	 |
|32                  |64                  |58                  |46                  |79%	 |
|32                  |128                 |117                 |88                  |75%	 |
|32                  |256                 |231                 |172                 |74%	 |
|32                  |512                 |461                 |330                 |71%	 |
|32                  |1024                |927                 |657                 |70%	 |
|32                  |2048                |1854                |1255                |67%	 |
|64                  |16                  |49                  |23                  |46%	 |
|64                  |32                  |87                  |46                  |52%	 |
|64                  |64                  |144                 |92                  |63%	 |
|64                  |128                 |288                 |179                 |62%	 |
|64                  |256                 |552                 |346                 |62%	 |
|64                  |512                 |652                 |699                 |107%	 |
|64                  |1024                |1306                |1397                |106%	 |
|64                  |2048                |2619                |2789                |106%	 |
|128                 |16                  |62                  |39                  |62%	 |
|128                 |32                  |100                 |88                  |88%	 |
|128                 |64                  |162                 |180                 |111%	 |
|128                 |128                 |251                 |369                 |147%	 |
|128                 |256                 |504                 |714                 |141%	 |
|128                 |512                 |1010                |1413                |139%	 |
|128                 |1024                |2025                |2892                |142%	 |
|128                 |2048                |4064                |5772                |142%	 |
|256                 |16                  |124                 |77                  |62%	 |
|256                 |32                  |201                 |174                 |86%	 |
|256                 |64                  |326                 |334                 |102%	 |
|256                 |128                 |503                 |724                 |143%	 |
|256                 |256                 |774                 |1492                |192%	 |
|256                 |512                 |1501                |2939                |195%	 |
|256                 |1024                |3111                |5962                |191%	 |
|256                 |2048                |5818                |11954               |205%	 |
|512                 |16                  |245                 |156                 |63%	 |
|512                 |32                  |403                 |318                 |78%	 |
|512                 |64                  |650                 |681                 |104%	 |
|512                 |128                 |994                 |1414                |142%	 |
|512                 |256                 |1549                |2903                |187%	 |
|512                 |512                 |2326                |6062                |260%	 |
|512                 |1024                |4701                |12078               |256%	 |
|512                 |2048                |8921                |23813               |266%	 |
|1024                |16                  |496                 |279                 |56%	 |
|1024                |32                  |806                 |653                 |81%	 |
|1024                |64                  |1309                |1420                |108%	 |
|1024                |128                 |2030                |2867                |141%	 |
|1024                |256                 |3114                |5932                |190%	 |
|1024                |512                 |4423                |11760               |265%	 |
|1024                |1024                |6811                |24268               |356%	 |
|1024                |2048                |13620               |47401               |348%	 |
|2048                |16                  |987                 |536                 |54%	 |
|2048                |32                  |1581                |1326                |83%	 |
|2048                |64                  |2617                |2828                |108%	 |
|2048                |128                 |3712                |5335                |143%	 |
|2048                |256                 |5789                |11281               |194%	 |
|2048                |512                 |9167                |24276               |264%	 |
|2048                |1024                |13829               |48808               |352%	 |
|2048                |2048                |19690               |99399               |504%	 |

## Conclusion
This method can provide better performance than BigInteger for values smaller than 1234 digits. 
Perhaps using this method, two 64-bit numbers can be multiplied simultaneously with AVX-256 and all performance lines can be achieved.
  
