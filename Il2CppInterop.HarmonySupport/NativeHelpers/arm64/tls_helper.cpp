#include <cstdint>
#include <pthread.h>

namespace {
pthread_key_t g_Arm64ReturnBufferKey;
pthread_once_t g_Arm64ReturnBufferKeyOnce = PTHREAD_ONCE_INIT;
bool g_Arm64ReturnBufferKeyReady = false;

void InitReturnBufferKey() {
    g_Arm64ReturnBufferKeyReady = (pthread_key_create(&g_Arm64ReturnBufferKey, nullptr) == 0);
}

bool EnsureReturnBufferKey() {
    if (pthread_once(&g_Arm64ReturnBufferKeyOnce, InitReturnBufferKey) != 0) {
        return false;
    }

    return g_Arm64ReturnBufferKeyReady;
}
} // namespace

extern "C" void SetReturnBuffer(intptr_t value) {
    if (!EnsureReturnBufferKey()) {
        return;
    }

    pthread_setspecific(g_Arm64ReturnBufferKey, reinterpret_cast<void*>(static_cast<uintptr_t>(value)));
}

extern "C" intptr_t GetReturnBuffer() {
    if (!EnsureReturnBufferKey()) {
        return 0;
    }

    return static_cast<intptr_t>(reinterpret_cast<uintptr_t>(pthread_getspecific(g_Arm64ReturnBufferKey)));
}
