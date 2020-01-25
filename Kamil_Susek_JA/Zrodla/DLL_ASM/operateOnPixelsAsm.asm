.code
;-------------------------------------------------------------------------------------------------------------------------------------------------
;/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
;-------------------------------------------------------------------------------------------------------------------------------------------------
operateOnPixelsAsm PROC 

	movdqu xmm1, [rbx + 0*SIZEOF BYTE]		; load vector 1	
	movdqu xmm2, [rdx + 0*SIZEOF BYTE]		; load vector 2

	psubb xmm1,xmm2							; subtract vectors

	pabsb xmm1,xmm1							; get absolute value

	movdqu [rcx],xmm1						; load absolute value to result array

	ret								
operateOnPixelsAsm ENDP

checkSSE PROC
		xor eax, eax
		cpuid

		test eax, eax
		jz NoCPUID
		mov eax, 1

		cpuid
		test edx, 2000000h

		jnz SSEAvailable
		mov eax,0
		ret

		NoCPUID:
		mov eax, -1
		ret

		SSEAvailable:
		mov eax,1
		ret
checkSSE ENDP

END									