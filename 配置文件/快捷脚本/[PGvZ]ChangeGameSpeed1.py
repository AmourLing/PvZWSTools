#2025.11.07
#修改植物娘运行倍速

from Lawn import *
from Sexy import *
from Sexy import GlobalStaticVars as G

app = G.gLawnApp
board = app.mBoard

def decimal_to_fraction(decimal_num, tolerance=1e-6, max_denominator=114514):
    #使用辗转相除法将小数转换为分数形式
    #处理符号（?不会真有人用负倍率吧）
    sign = 1
    if decimal_num < 0:
        sign = -1
        decimal_num = abs(decimal_num)
    
    #整数结果直接处理
    if abs(decimal_num - round(decimal_num)) < tolerance:
        return (sign * int(round(decimal_num)), 1)
    
    numerator = 1
    denominator = 0
    prev_numerator = 0
    prev_denominator = 1
    
    x = decimal_num
    
    while True:
        # 整数部分
        integer_part = int(x)
        
        # 更新分数
        new_numerator = integer_part * numerator + prev_numerator
        new_denominator = integer_part * denominator + prev_denominator
        
        # 检查分母是否超过限制
        if new_denominator > max_denominator:
            break
        
        # 更新前一个值
        prev_numerator, numerator = numerator, new_numerator
        prev_denominator, denominator = denominator, new_denominator
        
        # 检查是否达到精度
        if abs(decimal_num - numerator/denominator) < tolerance:
            break
        
        # 更新x
        fractional_part = x - integer_part
        if fractional_part < tolerance:
            break
        x = 1.0 / fractional_part
    
    # 应用符号
    numerator = sign * numerator
    
    return (numerator, denominator)

# 性能测试函数
def performance_test():
    import time
    
    test_values = [
        0.5, 
        0.25, 
        0.333333, 
        0.142857,
        0.123456, 
        2.75, 
        3.14159, 
        1.61803, 
        0.000001, 
        0.999999
    ]
    
    start_time = time.time()
    for val in test_values:
        for _ in range(1000):  # 重复多次以获得可测量的时间
            num, den = decimal_to_fraction(val, tolerance=1e-6)
    end_time = time.time()
    
    print("处理 {} 次转换用时: {:.6f} 秒".format(len(test_values) * 1000, end_time - start_time))
    
    # 显示转换结果
    for val in test_values:
        num, den = decimal_to_fraction(val, tolerance=1e-6)
        print("{:.6f} = {}/{} (误差: {:.2e})".format(
            val, num, den, abs(val - num/den)))

if __name__ == "__main__":
    performance_test()

a,b = decimal_to_fraction(10)
board.mAccelerationNumerator = a
board.mAccelerationDenominator = b

