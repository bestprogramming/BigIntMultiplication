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
| :------------------| :------------------| :------------------| :------------------| :----- |
|16                  |16                  |9                   |6                   |66%     |
|16                  |32                  |18                  |12                  |66%     |
|16                  |64                  |36                  |21                  |58%     |
|16                  |128                 |72                  |40                  |55%     |
|16                  |256                 |141                 |78                  |55%     |
|16                  |512                 |278                 |144                 |51%     |
|16                  |1024                |561                 |265                 |47%     |
|16                  |2048                |1128                |528                 |46%     |
|32                  |16                  |18                  |12                  |66%     |
|32                  |32                  |30                  |23                  |76%     |
|32                  |64                  |58                  |45                  |77%     |
|32                  |128                 |116                 |82                  |70%     |
|32                  |256                 |233                 |170                 |72%     |
|32                  |512                 |467                 |334                 |71%     |
|32                  |1024                |931                 |640                 |68%     |
|32                  |2048                |1871                |1323                |70%     |
|64                  |16                  |48                  |23                  |47%     |
|64                  |32                  |85                  |46                  |54%     |
|64                  |64                  |144                 |85                  |59%     |
|64                  |128                 |289                 |183                 |63%     |
|64                  |256                 |330                 |356                 |107%    |
|64                  |512                 |654                 |696                 |106%    |
|64                  |1024                |1308                |1390                |106%    |
|64                  |2048                |2612                |2709                |103%    |
|128                 |16                  |61                  |37                  |60%     |
|128                 |32                  |100                 |84                  |84%     |
|128                 |64                  |162                 |176                 |108%    |
|128                 |128                 |253                 |355                 |140%    |
|128                 |256                 |503                 |723                 |143%    |
|128                 |512                 |1009                |1405                |139%    |
|128                 |1024                |1996                |2834                |141%    |
|128                 |2048                |4039                |5794                |143%    |
|256                 |16                  |123                 |72                  |58%     |
|256                 |32                  |200                 |169                 |84%     |
|256                 |64                  |325                 |351                 |108%    |
|256                 |128                 |504                 |707                 |140%    |
|256                 |256                 |774                 |1467                |189%    |
|256                 |512                 |1543                |2963                |192%    |
|256                 |1024                |3046                |5860                |192%    |
|256                 |2048                |5796                |11690               |201%    |
|512                 |16                  |243                 |143                 |58%     |
|512                 |32                  |400                 |317                 |79%     |
|512                 |64                  |650                 |686                 |105%    |
|512                 |128                 |1010                |1451                |143%    |
|512                 |256                 |1545                |2954                |191%    |
|512                 |512                 |2341                |5972                |255%    |
|512                 |1024                |4377                |12077               |275%    |
|512                 |2048                |8760                |23290               |265%    |
|1024                |16                  |488                 |266                 |54%     |
|1024                |32                  |803                 |671                 |83%     |
|1024                |64                  |1278                |1398                |109%    |
|1024                |128                 |2024                |2849                |140%    |
|1024                |256                 |3083                |5869                |190%    |
|1024                |512                 |4467                |11852               |265%    |
|1024                |1024                |6603                |23285               |352%    |
|1024                |2048                |12974               |47030               |362%    |
|2048                |16                  |930                 |493                 |53%     |
|2048                |32                  |1602                |1293                |80%     |
|2048                |64                  |2607                |2777                |106%    |
|2048                |128                 |4032                |5703                |141%    |
|2048                |256                 |5770                |11730               |203%    |
|2048                |512                 |8731                |22610               |258%    |
|2048                |1024                |13186               |46620               |353%    |
|2048                |2048                |20239               |94075               |464%    |



## Conclusion
This method can provide better performance than BigInteger for values smaller than 1234 digits. 
Perhaps using this method, two 64-bit numbers can be multiplied simultaneously with AVX-256 and all performance lines can be achieved.
  
