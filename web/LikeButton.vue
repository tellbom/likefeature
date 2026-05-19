<template>
    <div class="like-button-wrapper">
        <button
            class="like-btn"
            :class="{ 'is-liked': liked, 'is-loading': loading }"
            :disabled="loading"
            @click="handleClick"
        >
            <span class="like-icon-wrap">
                <i v-if="loading" class="fa fa-circle-o-notch fa-spin"></i>
                <i v-else-if="liked" class="fa fa-thumbs-up"></i>
                <i v-else class="fa fa-thumbs-o-up"></i>
            </span>
            <span class="like-label">{{ liked ? '已点赞' : '点赞' }}</span>
            <span class="like-count-badge" :class="{ 'has-count': likeCount > 0 }">
                {{ formatCount(likeCount) }}
            </span>
        </button>
        <transition name="pop">
            <span v-if="showTip" class="like-tip">{{ tipText }}</span>
        </transition>
    </div>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue'

interface Props {
    newsId: string | number
    liked: boolean
    likeCount: number
    loading?: boolean
}

interface Emits {
    (e: 'toggle', newsId: string | number): void
}

const props = withDefaults(defineProps<Props>(), {
    loading: false
})

const emit = defineEmits<Emits>()

const showTip = ref(false)
const tipText = ref('')
let tipTimer: ReturnType<typeof setTimeout> | null = null

const formatCount = (count: number): string => {
    if (count >= 10000) return (count / 10000).toFixed(1) + 'w'
    return String(count)
}

const showToast = (text: string) => {
    tipText.value = text
    showTip.value = true
    if (tipTimer) clearTimeout(tipTimer)
    tipTimer = setTimeout(() => {
        showTip.value = false
    }, 1800)
}

// 监听 liked 变化，给出操作反馈
let prevLiked = props.liked
watch(
    () => props.liked,
    (val) => {
        if (val !== prevLiked) {
            showToast(val ? '点赞成功！' : '已取消点赞')
            prevLiked = val
        }
    }
)

const handleClick = () => {
    if (props.loading) return
    emit('toggle', props.newsId)
}
</script>

<style scoped lang="scss">
$primary: #0066cc;
$primary-light: #e6f0fb;
$primary-hover: #0052a3;
$primary-active: #004080;

.like-button-wrapper {
    display: inline-flex;
    flex-direction: column;
    align-items: center;
    gap: 6px;
    position: relative;
}

.like-btn {
    display: inline-flex;
    align-items: center;
    gap: 8px;
    padding: 10px 24px;
    border: 1.5px solid $primary;
    border-radius: 24px;
    background: #fff;
    color: $primary;
    font-size: 15px;
    font-family: inherit;
    cursor: pointer;
    transition: all 0.22s cubic-bezier(0.4, 0, 0.2, 1);
    user-select: none;
    outline: none;

    &:hover:not(:disabled) {
        background: $primary-light;
        border-color: $primary-hover;
        color: $primary-hover;
        transform: translateY(-1px);
        box-shadow: 0 4px 12px rgba(0, 102, 204, 0.18);
    }

    &:active:not(:disabled) {
        transform: translateY(0);
        box-shadow: none;
    }

    &:disabled {
        cursor: not-allowed;
        opacity: 0.7;
    }

    // 已点赞状态
    &.is-liked {
        background: $primary;
        color: #fff;
        border-color: $primary;

        &:hover:not(:disabled) {
            background: $primary-hover;
            border-color: $primary-hover;
            color: #fff;
            box-shadow: 0 4px 12px rgba(0, 102, 204, 0.28);
        }

        .like-count-badge {
            background: rgba(255, 255, 255, 0.25);
            color: #fff;
        }
    }
}

.like-icon-wrap {
    display: flex;
    align-items: center;
    font-size: 16px;
    transition: transform 0.2s;

    .like-btn:not(.is-loading):hover & {
        transform: scale(1.15);
    }
}

.like-label {
    font-weight: 500;
    letter-spacing: 0.02em;
}

.like-count-badge {
    display: inline-flex;
    align-items: center;
    justify-content: center;
    min-width: 22px;
    padding: 0 6px;
    height: 20px;
    border-radius: 10px;
    background: $primary-light;
    color: $primary;
    font-size: 12px;
    font-weight: 600;
    line-height: 1;
    transition: all 0.22s;

    &:not(.has-count) {
        opacity: 0.5;
    }
}

// 气泡提示
.like-tip {
    position: absolute;
    top: -36px;
    left: 50%;
    transform: translateX(-50%);
    background: rgba(0, 0, 0, 0.72);
    color: #fff;
    font-size: 12px;
    padding: 4px 12px;
    border-radius: 12px;
    white-space: nowrap;
    pointer-events: none;
}

// 提示出入动画
.pop-enter-active,
.pop-leave-active {
    transition: opacity 0.2s, transform 0.2s;
}
.pop-enter-from,
.pop-leave-to {
    opacity: 0;
    transform: translateX(-50%) translateY(4px);
}
</style>
