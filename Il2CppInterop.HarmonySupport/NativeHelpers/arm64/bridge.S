.section .text
.global ReturnBufferBridge
.type ReturnBufferBridge, %function

ReturnBufferBridge:
    // X0..X7 = Original Arguments (this, parent, etc.)
    // X8     = The Return Buffer Pointer
    // X16    = The address of the Managed Trampoline

    // 1. Preserve every integer and SIMD argument register before calling C++.
    // SetReturnBuffer follows AAPCS64 and may clobber all caller-saved registers.
    stp x30, x16, [sp, #-16]!
    stp x6,  x7,  [sp, #-16]!
    stp x4,  x5,  [sp, #-16]!
    stp x2,  x3,  [sp, #-16]!
    stp x0,  x1,  [sp, #-16]!
    stp q6,  q7,  [sp, #-32]!
    stp q4,  q5,  [sp, #-32]!
    stp q2,  q3,  [sp, #-32]!
    stp q0,  q1,  [sp, #-32]!

    // 2. Pass the Return Buffer Pointer (X8) to TLS storage for this thread
    mov x0, x8
    bl  SetReturnBuffer

    // 3. Restore the saved registers from the stack
    ldp q0,  q1,  [sp], #32
    ldp q2,  q3,  [sp], #32
    ldp q4,  q5,  [sp], #32
    ldp q6,  q7,  [sp], #32
    ldp x0,  x1,  [sp], #16
    ldp x2,  x3,  [sp], #16
    ldp x4,  x5,  [sp], #16
    ldp x6,  x7,  [sp], #16
    ldp x30, x16, [sp], #16

    // 4. Jump to the managed trampoline address we stored in X16
    br  x16
