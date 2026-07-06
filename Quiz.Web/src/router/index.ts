import { createRouter, createWebHistory } from 'vue-router'
import { authGuard } from '@guards/authGuard'
import { setTitle } from '@guards/setTitle'

export const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      name: 'home',
      component: () => import('@pages/HomePage.vue'),
      meta: {
        requiresAuth: false,
        title: 'ELTE MWF Quiz | Home'
      }
    },
    {
      path: '/login',
      name: 'login',
      component: () => import('@pages/LoginPage.vue'),
      meta: {
        requiresAuth: false,
        title: 'ELTE MWF Quiz | Login'
      }
    },
    {
      path: '/register',
      name: 'register',
      component: () => import('@pages/RegisterPage.vue'),
      meta: {
        requiresAuth: false,
        title: 'ELTE MWF Quiz | Register'
      }
    },
    {
      path: '/myAccount',
      name: 'myAccount',
      component: () => import('@pages/MyAccountPage.vue'),
      meta: {
        requiresAuth: true,
        title: 'ELTE MWF Quiz | MyAccount'
      }
    },
    {
      path: '/myQuiz/:quizId',
      name: 'myQuiz',
      component: () => import('@pages/MyQuizPage.vue'),
      meta: {
        requiresAuth: true,
        title: 'ELTE MWF Quiz | My Quiz'
      }
    },
    {
      path: '/quizzes/:quizId/userActiveQuestion',
      name: 'userActiveQuestion',
      component: () => import('@pages/UserActiveQuestionPage.vue'),
      meta: {
        requiresAuth: true,
        title: 'ELTE MWF Quiz | User Active Question'
      }
    },
    {
      path: '/quizzes/:quizId/activeQuestion',
      name: 'activeQuestion',
      component: () => import('@pages/ActiveQuestionPage.vue'),
      meta: {
        requiresAuth: false,
        title: 'ELTE MWF Quiz | Active Question'
      }
    },
    {
      path: '/quizzes/create',
      name: 'createQuiz',
      component: () => import('@pages/CreateQuizPage.vue'),
      meta: {
        requiresAuth: true,
        title: 'ELTE MWF Quiz | Create Quiz'
      }
    }
  ],
  linkActiveClass: 'active'
})
router.beforeEach((to, from) => {
  const result = authGuard(to, from)
  if (result !== true) return result
  setTitle(to)
  return true
});
