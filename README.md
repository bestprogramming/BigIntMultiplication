# BigInt Multiplication
BigInt Multiplication provides an example for multiplying two 64-bit arrays. 
Performs a total of leftLength x rightLength multiplication operations.
It does not require the result array to be reset while the calculation is performed and sequential access is provided while reaching the result.
Performing multiplication sequentially may be more amenable to parallel computation in the future.
Especially if features like AVX, 64 bit multiplication and writing the result as low and high bits and addition with carry are included. Of course, loading the registers (ymm, zmm) from memory in reverse order can bring an extra performance increase.

## Decimal digits count of 64-bit arrays
When performing mathematical operations, we may need operations with higher precision than numbers such as double or float. For example, if we want to find the roots of a quartic function as a double, the input of the function must be greater than the double precision.

| Length | Byte Count | Decimal Count |
|--------|----------------------------|
|  8     |  64        |  155          |
|  16    |  128       |  309          |
|  32    |  256       |  617          |
|  64    |  512       |  1234         |
|  128   |  1024      |  2467         |
|  256   |  2048      |  4933         |
|  512   |  4096      |  9865         |
|  1024  |  8192      |  19719        |
|  2048  |  16384     |  39447        |





