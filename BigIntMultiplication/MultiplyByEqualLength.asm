; esi: length
; r8: left
; r9: right
; rdi: result

.code
; r10d: leftIndex
; r11d: rightIndex
; r12d: resultIndex


MultiplyByEqualLength proc export
;length1
	cmp ecx, 1
	jnz length1_end
	mov rax, [rdx]
	mov rdx, [r8]
	mul rdx
	mov [r9], rax
	cmp rdx, 0
	jz retLength1
	mov [r9 + 8], rdx
	mov eax, 2
	ret
retLength1:
	mov eax, 1
	ret
length1_end:



	mov esi, ecx
	mov rdi, r9 
	mov r9, r8
	mov r8, rdx

	xor r12, r12

	xor r13, r13
	xor r14, r14
	xor r15, r15


	
	
	mov ecx, esi
start:
	xor r10d, r10d
	mov r11d, r12d

start_inner:
	mov rax, [r8 + r10 * 8]
	mov rdx, [r9 + r11 * 8]
	mul rdx

;add_rax
	add r13, rax
;add_rax_end

;add_rdx_start
	adc r14, rdx
	jnc add_rdx_start_end
;carry
	inc r15d
add_rdx_start_end:

	inc r10d
	dec r11d
	jns start_inner
;start_inner_end
	mov [rdi], r13
	mov r13, r14
	mov r14, r15
	xor r15, r15	


	inc r12d
	lea rdi, [rdi + 8]
	dec ecx
	jnz start
;start_end




	dec esi
	mov ecx, esi
	mov r12d, 1
finish:

	mov r10d, r12d
	mov r11d, esi
finish_inner:
	mov rax, [r8 + r10 * 8]
	mov rdx, [r9 + r11 * 8]
	mul rdx

;add_rax
	add r13, rax
;add_rax_end

;add_rdx_finish
	adc r14, rdx
	jnc add_rdx_finish_end
;carry
	inc r15d
add_rdx_finish_end:	

	inc r10d
	dec r11d
	cmp r11d, r12d
	jae finish_inner
;finish_inner_end
	mov [rdi], r13
	mov r13, r14
	mov r14, r15
	xor r15, r15	



	inc r12d
	lea rdi, [rdi + 8]
	dec ecx
	jnz finish
;finish_end


	mov eax, esi
	add eax, eax
	inc eax

;add_r13
	cmp r13, 0
	jz add_r13_end
	inc eax
	mov [rdi], r13
add_r13_end:

;add_r14
	cmp r14, 0
	jz add_r14_end
	inc eax
	mov [rdi + 8], r14
add_r14_end:


	ret
MultiplyByEqualLength endp

end