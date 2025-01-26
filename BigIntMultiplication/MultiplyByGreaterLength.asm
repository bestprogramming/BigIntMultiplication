; esi: leftLength
; ebp: rightLength
; r8: left
; r9: right
; rdi: result

.code
; r10d: leftIndex
; r11d: rightIndex
; r12d: resultIndex


MultiplyByGreaterLength proc export
	mov rdi, [rsp + 40]

;rightLength1
	cmp edx, 1
	jnz rightLength1_end
	push rcx
	mov r9, [r9]
	xor r13, r13
do_while:
	mov rax, [r8]
	mov rdx, r9
	mul rdx

;add_rax
	add r13, rax
	mov [rdi], r13
;add_rax_end

	mov r13, rdx

	lea r8, [r8 + 8]
	lea rdi, [rdi + 8]
		
	dec ecx
	jnz do_while
;do_while_end
	pop rax
;add_rdx
	cmp rdx, 0
	jz add_rdx_end
	inc eax
	mov [rdi], rdx
add_rdx_end:	
	ret
rightLength1_end:


	push rbp

	mov esi, ecx	
	mov ebp, edx

	xor r12, r12

	xor r13, r13
	xor r14, r14
	xor r15, r15


	
	
	mov ecx, ebp
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

;add_rdx_start:
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







	mov ecx, esi
	sub ecx, ebp
	dec ebp
	mov r12d, 1
middle:
	mov r10d, r12d
	mov r11d, ebp
middle_inner:	
	mov rax, [r8 + r10 * 8]
	mov rdx, [r9 + r11 * 8]
	mul rdx

;add_rax
	add r13, rax
;add_rax_end

;add_rdx_middle
	adc r14, rdx
	jnc add_rdx_middle_end
;carry
	inc r15d
add_rdx_middle_end:

	inc r10d
	dec r11d
	jns middle_inner
;middle_inner_end
	mov [rdi], r13
	mov r13, r14
	mov r14, r15
	xor r15, r15

	inc r12d
	lea rdi, [rdi + 8]
	dec ecx
	jnz middle
;middle_end





	mov ecx, ebp
finish:

	mov r10d, r12d
	mov r11d, ebp
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
	cmp r10d, esi
	jb finish_inner
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
	add eax, ebp

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

	pop rbp
	ret
MultiplyByGreaterLength endp

end