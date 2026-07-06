import type { RouteLocationNormalizedLoadedGeneric } from 'vue-router'
import { useAuth } from '@stores/authStore'
import { useQuizzes } from '@stores/quizzesStore'

export function authGuard(
    to: RouteLocationNormalizedLoadedGeneric,
    from: RouteLocationNormalizedLoadedGeneric
) {
    const authStore = useAuth()
    const quizStore = useQuizzes()

    if (to.meta.requiresAuth && !authStore.user) {
        return { name: 'login' }
    }

    if (authStore.user && ['login', 'register'].includes(to.name as string)) {
        return { name: 'myAccount' }
    }

    if(to.name == 'activeQuestion'){
        if(!quizStore.joinedQuizId){
            return {name : 'home'}
        }
        
        const quizId = Number(to.params.quizId)
        if(Number.isNaN(quizId)){
            return {name: 'home'}
        }

        if(quizStore.joinedQuizId != quizId){
            return {name : 'home'}
        }
    }
   

    return true
}