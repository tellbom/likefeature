<template>
    <div class="news-detail-page">
        <!-- 面包屑导航 -->
        <div class="breadcrumb-section">
            <div class="breadcrumb-wrapper">
                <span class="breadcrumb-label">当前位置：</span>
                <el-breadcrumb separator=">">
                    <el-breadcrumb-item :to="{ path: '/' }">
                        <i class="fa fa-home"></i>
                    </el-breadcrumb-item>
                    <el-breadcrumb-item :to="{ path: '/search/index' }">
                        新闻中心
                    </el-breadcrumb-item>
                    <el-breadcrumb-item v-if="newsDetail.categoryName">
                        {{ newsDetail.categoryName }}
                    </el-breadcrumb-item>
                    <el-breadcrumb-item>正文</el-breadcrumb-item>
                </el-breadcrumb>
            </div>
        </div>

        <!-- 加载状态 -->
        <div v-if="loading" class="loading-wrapper">
            <el-icon class="is-loading" :size="40">
                <Loading />
            </el-icon>
            <p>加载中...</p>
        </div>

        <!-- 新闻详情 -->
        <div v-else-if="newsDetail.id" class="news-detail-container">
            <!-- 文章头部 -->
            <div class="article-header">
                <h1 class="article-title">{{ newsDetail.title }}</h1>
                <div class="article-meta">
                    <span class="meta-item">
                        <i class="fa fa-folder-open"></i>
                        {{ newsDetail.categoryName }}
                    </span>
                    <span class="meta-item">
                        <i class="fa fa-user"></i>
                        {{ newsDetail.author || '佚名' }}
                    </span>
                    <span class="meta-item">
                        <i class="fa fa-clock"></i>
                        {{ formatDate(newsDetail.publishTime) }}
                    </span>
                </div>
                <div class="divider"></div>
            </div>

            <!-- 文章内容 -->
            <div class="article-content" v-html="processedContent"></div>

            <!-- 点赞区域 -->
            <div class="article-like-section">
                <LikeButton
                    :news-id="newsDetail.id"
                    :liked="likeState.liked"
                    :like-count="likeState.likeCount"
                    :loading="likeState.loading"
                    @toggle="handleLikeToggle"
                />
            </div>

            <!-- 返回按钮 -->
            <div class="article-footer">
                <el-button @click="handleBack">
                    <i class="fa fa-arrow-left"></i>
                    返回列表
                </el-button>
            </div>
        </div>

        <!-- 错误状态 -->
        <div v-else class="error-wrapper">
            <el-empty description="新闻不存在或已被删除">
                <el-button type="primary" @click="handleBack">返回列表</el-button>
            </el-empty>
        </div>
    </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, reactive } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import dayjs from 'dayjs'
import { Loading } from '@element-plus/icons-vue'
import LikeButton from './LikeButton.vue'

interface NewsDetail {
    id: string | number
    title: string
    content: string
    author: string
    publishTime: string
    categoryName: string
}

interface LikeState {
    liked: boolean
    likeCount: number
    loading: boolean
}

const route = useRoute()
const router = useRouter()

// 新闻加载状态
const loading = ref(false)
const newsDetail = ref<NewsDetail>({
    id: '',
    title: '',
    content: '',
    author: '',
    publishTime: '',
    categoryName: ''
})

// 点赞状态（父组件持有，通过 props 传给子组件）
const likeState = reactive<LikeState>({
    liked: false,
    likeCount: 0,
    loading: false
})

// 处理后的内容
const processedContent = computed(() => {
    if (!newsDetail.value.content) return ''
    const tempDiv = document.createElement('div')
    tempDiv.innerHTML = newsDetail.value.content
    const images = tempDiv.querySelectorAll('img')
    images.forEach(img => {
        img.removeAttribute('style')
        img.removeAttribute('class')
        img.removeAttribute('width')
        img.removeAttribute('height')
        img.classList.add('content-image')
        const wrapper = document.createElement('div')
        wrapper.className = 'image-wrapper'
        img.parentNode?.insertBefore(wrapper, img)
        wrapper.appendChild(img)
    })
    const paragraphs = tempDiv.querySelectorAll('p')
    paragraphs.forEach(p => {
        const style = p.getAttribute('style')
        if (style) {
            const textAlign = style.match(/text-align:\s*(\w+)/)
            if (textAlign) {
                p.setAttribute('style', `text-align: ${textAlign[1]}`)
            } else {
                p.removeAttribute('style')
            }
        }
    })
    const tables = tempDiv.querySelectorAll('table')
    tables.forEach(table => {
        table.classList.add('content-table')
        const wrapper = document.createElement('div')
        wrapper.className = 'table-wrapper'
        table.parentNode?.insertBefore(wrapper, table)
        wrapper.appendChild(table)
    })
    return tempDiv.innerHTML
})

const formatDate = (dateStr: string) => {
    if (!dateStr) return ''
    return dayjs(dateStr).format('YYYY年MM月DD日 HH:mm')
}

// ─── 点赞 ────────────────────────────────────────────
// 获取当前用户的点赞状态（初始化时调用）
const fetchLikeState = async (newsId: string | number) => {
    try {
        // TODO: 替换为真实 API
        // GET /api/likes/status?newsId=xxx  Header: Authorization: Bearer <token>
        // GET /api/likes/count?newsId=xxx

        // Mock：模拟从接口拿到初始状态
        await new Promise(resolve => setTimeout(resolve, 200))
        likeState.liked     = MOCK_LIKE_DATA[newsId]?.liked     ?? false
        likeState.likeCount = MOCK_LIKE_DATA[newsId]?.likeCount ?? 0
    } catch (e) {
        console.error('获取点赞状态失败', e)
    }
}

// 点赞 toggle：子组件 emit('toggle') 后由父组件调用接口
const handleLikeToggle = async (newsId: string | number) => {
    if (likeState.loading) return
    likeState.loading = true

    try {
        // TODO: 替换为真实 API
        // POST /api/likes/toggle  Header: Authorization: Bearer <token>  Body: { newsId }
        // 接口返回 { success, liked, likeCount, eventType }

        // Mock：模拟接口延迟
        await new Promise(resolve => setTimeout(resolve, 400))

        // Mock 本地翻转（真实场景用接口返回值赋值）
        likeState.liked     = !likeState.liked
        likeState.likeCount = likeState.liked
            ? likeState.likeCount + 1
            : Math.max(0, likeState.likeCount - 1)
    } catch (e) {
        console.error('点赞操作失败', e)
    } finally {
        likeState.loading = false
    }
}

// ─── Mock 数据 ────────────────────────────────────────
// 真实接入后删除此段，改为调用 /api/likes/status 和 /api/likes/count
const MOCK_LIKE_DATA: Record<string | number, { liked: boolean; likeCount: number }> = {
    '1':    { liked: false, likeCount: 128 },
    '2':    { liked: true,  likeCount: 356 },
    '3':    { liked: false, likeCount: 12  },
    // 默认兜底
    default: { liked: false, likeCount: 0 }
}

// ─── 新闻详情 ─────────────────────────────────────────
const fetchNewsDetail = async (id: string | number) => {
    loading.value = true
    try {
        await new Promise(resolve => setTimeout(resolve, 500))
        newsDetail.value = {
            id: id,
            title: '习近平会见加纳总统马哈马',
            content: `<p>国家主席习近平在人民大会堂会见加纳总统马哈马。</p>
                      <p>习近平表示，中加两国是好朋友、好兄弟、好伙伴。</p>`,
            author: '新华社',
            publishTime: '2025-10-15 09:30:00',
            categoryName: '中央声音'
        }
        // 新闻加载完成后，同步拉取点赞状态
        await fetchLikeState(id)
    } catch (error) {
        console.error('获取新闻详情失败:', error)
    } finally {
        loading.value = false
    }
}

const handleBack = () => {
    router.back()
}

const setupMessageListener = () => {
    window.addEventListener('message', (e) => {
        newsDetail.value = {
            id: 'preview',
            title: '内容预览',
            content: '<p style="text-align: center; color: #999;">正在加载预览内容...</p>',
            author: '预览',
            publishTime: new Date().toISOString(),
            categoryName: '预览模式'
        }
        if (e.data?.type === 'SET_CONTENT_HTML') {
            const html = e.data.html || '<p style="text-align: center; color: #999;">（空内容）</p>'
            newsDetail.value.content = html
            newsDetail.value.title = e.data.title || '内容预览'
            if (e.data.author) newsDetail.value.author = e.data.author
            if (e.data.categoryName) newsDetail.value.categoryName = e.data.categoryName
        }
    })
    window.parent.postMessage({ type: 'PREVIEW_READY' }, '*')
}

onMounted(() => {
    const newsId = route.query.id || route.params.id
    if (newsId) {
        fetchNewsDetail(newsId as string)
    } else {
        setupMessageListener()
    }
})
</script>

<style scoped lang="scss">
.news-detail-page {
    width: 100%;
    max-width: 1800px;
    margin: 0 auto;
    padding: 0 20px 60px;
    background: #ffffff;
}

.breadcrumb-section {
    padding: 20px 0;
    border-bottom: 1px solid #e5e5e5;
    margin-bottom: 32px;
}

.breadcrumb-wrapper {
    display: flex;
    align-items: center;
    font-size: 14px;
}

.breadcrumb-label {
    color: #666;
    margin-right: 8px;
}

:deep(.el-breadcrumb__item) {
    .el-breadcrumb__inner {
        color: #666;
        font-weight: 400;
        transition: color 0.3s;
        &:hover { color: #c62f2f; }
        i { margin-right: 4px; }
    }
    &:last-child .el-breadcrumb__inner {
        color: #1d1d1f;
        font-weight: 500;
    }
}

.loading-wrapper,
.error-wrapper {
    display: flex;
    flex-direction: column;
    align-items: center;
    justify-content: center;
    padding: 80px 0;
    .el-icon { color: #c62f2f; margin-bottom: 16px; }
    p { color: #666; font-size: 14px; }
}

.news-detail-container { background: #ffffff; }

.article-header { margin-bottom: 40px; }

.article-title {
    font-size: 32px;
    font-weight: 700;
    color: #1d1d1f;
    line-height: 1.4;
    margin: 0 0 24px;
    letter-spacing: -0.02em;
}

.article-meta {
    display: flex;
    flex-wrap: wrap;
    gap: 24px;
    margin-bottom: 24px;
}

.meta-item {
    display: flex;
    align-items: center;
    gap: 6px;
    font-size: 14px;
    color: #666;
    i { color: #c62f2f; font-size: 14px; }
}

.divider {
    height: 1px;
    background: linear-gradient(90deg, #c62f2f 0%, rgba(198, 47, 47, 0.3) 50%, transparent 100%);
}

.article-content {
    font-size: 16px;
    line-height: 1.8;
    color: #333;
    word-wrap: break-word;

    :deep(p) {
        font-size: 16px;
        line-height: 1.8;
        margin-bottom: 20px;
        text-indent: 2em;
        color: #333;
        &:first-child { margin-top: 0; }
        &:last-child  { margin-bottom: 0; }
        &:empty       { margin-bottom: 0; }
    }
}

// 点赞区域
.article-like-section {
    display: flex;
    justify-content: center;
    padding: 40px 0 8px;
}

.article-footer {
    margin-top: 32px;
    padding-top: 32px;
    border-top: 1px solid #e5e5e5;
    display: flex;
    justify-content: center;
    .el-button {
        min-width: 120px;
        i { margin-right: 6px; }
    }
}

@media (max-width: 768px) {
    .news-detail-page { padding: 0 16px 40px; }
    .article-title { font-size: 24px; margin-bottom: 16px; }
    .article-meta { flex-direction: column; gap: 12px; }
}
</style>
