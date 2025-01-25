# BigInt Multiplication
BigInt Multiplication provides an example for multiplying two 64-bit arrays. 
Performs a total of leftLength x rightLength multiplication operations.
It does not require the result array to be reset while the calculation is performed and sequential access is provided while reaching the result.
Performing multiplication sequentially may be more amenable to parallel computation in the future.
Especially if features like AVX, 64 bit multiplication and writing the result as low and high bits and addition with carry are included. Of course, loading the registers (ymm, zmm) from memory in reverse order can bring an extra performance increase.