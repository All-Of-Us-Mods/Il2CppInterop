#include <cstdint>

extern "C" thread_local intptr_t g_Arm64ReturnBuffer = 0;

extern "C" __attribute__((naked)) void CaptureX8ToTLS() {
    __asm__ (
        // 1. Get the thread pointer (TPIDR_EL0)
        // On Android/ARM64, the thread pointer is in this system register
        "mrs x9, tpidr_el0\n"

        // 2. Find the offset for g_Arm64ReturnBuffer
        // This usually requires the linker to help us with an ADRP/LDR
        // sequence to find the offset from the thread pointer.
        "adrp x10, :gottprel:g_Arm64ReturnBuffer\n"
        "ldr  x10, [x10, :gottprel_lo12:g_Arm64ReturnBuffer]\n"

        // 3. Store X8 into the calculated TLS address [TP + Offset]
        "str  x8, [x9, x10]\n"

        // 4. Return to the bridge
        "ret\n"
    );
}

extern "C" intptr_t GetReturnBuffer() {
    return g_Arm64ReturnBuffer;
}

extern "C" void SetReturnBuffer(intptr_t value) {
    g_Arm64ReturnBuffer = value;
}
