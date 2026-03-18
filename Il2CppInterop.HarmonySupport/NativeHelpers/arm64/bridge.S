.section .text
.global ReturnBufferBridge
.type ReturnBufferBridge, %function

ReturnBufferBridge:
    // X0..X7 = Original Arguments (this, parent, etc.)
    // X8     = The Return Buffer Pointer
    // X10    = The address of the Managed Trampoline

    // 1. Save the Link Register (LR) on the stack to preserve the return address
    str x30, [sp, #-16]

    // 2. Copy the return buffer to TLS
    bl  CaptureX8ToTLS

    // 3. Restore the Link Register before jumping to the managed trampoline
    ldr x30, [sp], #16

    // 4. Jump to the managed trampoline address we stored in X10
    br  x10
