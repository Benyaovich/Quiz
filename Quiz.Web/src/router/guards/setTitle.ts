import type { RouteLocationNormalizedLoadedGeneric } from 'vue-router'

export function setTitle(to: RouteLocationNormalizedLoadedGeneric){
    document.title = to.meta.title as string
}